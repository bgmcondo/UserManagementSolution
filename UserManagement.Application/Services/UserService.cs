
using Microsoft.AspNetCore.Identity;
using UserManagement.Application.Common;
using UserManagement.Application.DTOs;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Mappings;
using UserManagement.Domain.Entities;


namespace UserManagement.Application.Services
{
    // Kullanıcı işlemlerinin iş mantığı (IUserService implementasyonu).
    //
    // Sorumlulukları:
    //   - Girdiyi doğrulamak (FluentValidation)
    //   - Veriyi okumak/yazmak (IUserRepository)
    //   - Redis cache'i yönetmek: okurken cache-aside, yazarken invalidation (ICacheService)
    //   - Yeni kullanıcıda RabbitMQ'ya event bırakmak (IPublishEndpoint)
    //   - Admin'lere canlı bildirim göndermek (INotificationService)
    // Tüm bağımlılıklar arayüz olarak alınır. Bu sınıf Redis'i, PostgreSQL'i, RabbitMQ'yu
    // veya SignalR'ı doğrudan bilmez; testlerde hepsi mock'lanabilir.
    //validationservice kullanıcının gönderdiği veriyi kontrol etmek için var
    //Get metotlarında kontrol edilecek böyle bir veri yok.
    public class UserService : IUserService
    {
        private const string DefaultRole = "User";
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cache;
        private readonly IValidationService _validationService;




        public UserService(IUserRepository userRepository, ICacheService cacheService,IValidationService validationService )                       // ← mevcutlar aynen kalsın
        
        {
            _userRepository = userRepository;
            _cache = cacheService;
            _validationService = validationService;
           
        }


        public async Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct)
        {
            //  Önce Redis'e bakıyoruz
            var cached = await _cache.GetAsync<List<UserResponseDto>>(CacheKeys.AllUsers, ct);
            if (cached is not null)
                return cached;                       // DB'ye hiç gitmedik

            //  Redis'te yok ise repodan çekiyoruz
            var users = await _userRepository.GetAllAsync(ct);
            var dtos = users.Select(u => u.ToResponseDto()).ToList();

            // 3) Sonraki istekler için Redis'e yazıyoruz
            await _cache.SetAsync(CacheKeys.AllUsers, dtos, cancellationToken: ct);

            return dtos;
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await GetUserOrThrowAsync(id, ct);
            return user.ToResponseDto();
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserRequestDto createUserRequestDto, CancellationToken ct)
        {
            // Kurallara uymuyorsa ValidationException fırlatır, aşağısı HİÇ çalışmaz → middleware 400 döner
            await _validationService.ValidateAsync(createUserRequestDto, ct);   
            var existingUser = await _userRepository.GetByEmailAsync(createUserRequestDto.Email, ct);
            if (existingUser is not null)
                throw new ConflictException("Bu e-postaya sahip kullanıcı zaten var.");

            var user = new AppUser
            {
                UserName = createUserRequestDto.UserName,
                Email = createUserRequestDto.Email,
                FullName = createUserRequestDto.FullName,
                PhoneNumber = createUserRequestDto.PhoneNumber,
                Website = createUserRequestDto.Website,
                Address = createUserRequestDto.Address?.ToEntity(),
                Company = createUserRequestDto.Company?.ToEntity(),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.CreateAsync(user, createUserRequestDto.Password, DefaultRole, ct);
            ThrowIfFailed(result, "Kullanıcı oluşturulamadı");
            // Liste cache'leri artık eksik, hepsini temizliyoruz
            await _cache.RemoveByPrefixAsync(CacheKeys.UserListPrefix, ct);

            return user.ToResponseDto();
        }

        public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto updateUserRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(updateUserRequestDto,ct);
            var user = await GetUserOrThrowAsync(id, ct);

            // Önce değerleri entity'ye aktar, sonra kaydet
            user.FullName = updateUserRequestDto.FullName;
            user.PhoneNumber =updateUserRequestDto.PhoneNumber;
            user.Website = updateUserRequestDto.Website;
            user.Address = updateUserRequestDto.Address?.ToEntity();
            user.Company = updateUserRequestDto.Company?.ToEntity();
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user, ct);
            ThrowIfFailed(result, "Kullanıcı güncellenemedi");
            await InvalidateUserCacheAsync(id, ct);
            return user.ToResponseDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var user = await GetUserOrThrowAsync(id, ct);

            var result = await _userRepository.DeleteAsync(user, ct);

            // Silme hatası istemcinin girdisinden kaynaklanmaz (genelde eşzamanlılık/DB hatası) → 500
            if (!result.Succeeded)
                throw new InvalidOperationException($"Kullanıcı silinemedi ({id}): {JoinErrors(result)}");
            // Silme başarılı: hem tek kaydı hem listeleri cache'ten temizle
            await InvalidateUserCacheAsync(id, ct);
        }

        public async Task ChangeUserNameAsync(Guid userId, ChangeUserNameRequestDto changeUserNameRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(changeUserNameRequestDto, ct);
            var user = await GetUserOrThrowAsync(userId, ct);

            if (!await _userRepository.CheckPasswordAsync(user, changeUserNameRequestDto.CurrentPassword, ct))
                throw new ValidationException("Mevcut parola yanlış.");

            var result = await _userRepository.SetUserNameAsync(user, changeUserNameRequestDto.NewUserName, ct);
            ThrowIfFailed(result, "Kullanıcı adı değiştirilemedi");
            // UserName DTO'da görünüyor: tek kaydı ve listeleri temizler
           await InvalidateUserCacheAsync(userId, ct);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto changePasswordRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(changePasswordRequestDto, ct);
            var user = await GetUserOrThrowAsync(userId, ct);

            var result = await _userRepository.ChangePasswordAsync(user, changePasswordRequestDto.CurrentPassword, changePasswordRequestDto.NewPassword, ct);
            ThrowIfFailed(result, "Parola değiştirilemedi");
            // Cache temizlenmiyor: parola UserResponseDto'da yok, cache'teki veri değişmedi
        }

        // ---------- Yardımcılar ----------

        // Kullanıcıyı bulur, yoksa NotFoundException (404) fırlatır.
        private async Task<AppUser> GetUserOrThrowAsync(Guid id, CancellationToken ct) =>
            await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Kullanıcı bulunamadı: {id}");

        // IdentityResult başarısızsa: Duplicate* → 409, diğerleri → 400
        private static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (result.Succeeded)
                return;

            var errors = JoinErrors(result);

            if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
                throw new ConflictException($"{message}: {errors}");

            throw new ValidationException($"{message}: {errors}");
        }

        private static string JoinErrors(IdentityResult result) =>
            string.Join(", ", result.Errors.Select(e => e.Description));

        // Bir kullanıcı değişince: onun tek kaydını ve tüm listeleri cache'ten sil
        private async Task InvalidateUserCacheAsync(Guid id, CancellationToken ct)
        {
            await _cache.RemoveAsync(CacheKeys.User(id), ct);
            await _cache.RemoveByPrefixAsync(CacheKeys.UserListPrefix, ct);
        }

    }
}