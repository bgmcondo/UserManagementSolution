
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common;
using UserManagement.Application.DTOs;
using UserManagement.Application.Events;
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

        private const string AdminRole = "Admin";
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cache;
        private readonly IValidationService _validationService;

        private readonly INotificationService _notificationService;

        private readonly IJwtTokenService _tokenService;

        // Mesajı kuyruğa bırakmak için (MassTransit). Nereye gideceğini (RabbitMQ) Infrastructure bilir.
        private readonly IPublishEndpoint _publishEndpoint;

     




        public UserService(IUserRepository userRepository, ICacheService cacheService, IValidationService validationService, INotificationService notificationService,IJwtTokenService tokenService, IPublishEndpoint publishEndpoint)                       // ← mevcutlar aynen kalsın

        {
            _userRepository = userRepository;
            _cache = cacheService;
            _tokenService = tokenService;
            _validationService = validationService;
            _notificationService = notificationService;
            _publishEndpoint = publishEndpoint;

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
            // E-posta/kullanıcı adı benzersizliğini Identity kontrol ediyor (RequireUniqueEmail = true) → 409

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
            // Hoş geldin e-postası için kuyruğa mesaj bırak.
            // E-postanın gönderilmesini BEKLEMİYORUZ: UserCreatedConsumer arka planda gönderir,
            // e-posta sunucusu yavaş/kapalıysa bile bu istek hemen döner.
            await _publishEndpoint.Publish(
                new UserCreatedEvent(user.Id, user.Email!, user.FullName, user.CreatedAt), ct);
            // Liste cache'leri artık eksik, hepsini temizliyoruz
            await _cache.RemoveByPrefixAsync(CacheKeys.UserListPrefix, ct);
            var response = user.ToResponseDto();
            await _notificationService.NotifyUserCreatedAsync(response, ct);
            return response;

        }

        public async Task<UserResponseDto> UpdateAsync(Guid id,Guid currentUserId ,UpdateUserRequestDto updateUserRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(updateUserRequestDto, ct);
            var user = await GetUserOrThrowAsync(id, ct);

            await EnsureCanManageAsync(user, currentUserId, ct);

            // Önce değerleri entity'ye aktar, sonra kaydet
            // E-posta gerçekten değiştiyse: yeni adres doğrulanmamış sayılır ve eski token'lar geçersiz olur.
            // SetEmailAsync yerine burada yapıyoruz ki aşağıdaki tek UpdateAsync hepsini birlikte kaydetsin (1 UPDATE).
            var emailChanged = !string.Equals(user.Email, updateUserRequestDto.Email, StringComparison.OrdinalIgnoreCase);

            user.Email = updateUserRequestDto.Email;

            if (emailChanged)
            {
      
                //Securitystamp güvenlik bilgisi sürümü anlamına gelir.eposta değiştiğinde stamp de manuel olarak güncellenir ve eski tokenlar geçersiz olur
                user.SecurityStamp = Guid.NewGuid().ToString();  
            }
            user.FullName = updateUserRequestDto.FullName;
            user.PhoneNumber = updateUserRequestDto.PhoneNumber;
            user.Website = updateUserRequestDto.Website;
            user.Address = updateUserRequestDto.Address?.ToEntity();
            user.Company = updateUserRequestDto.Company?.ToEntity();
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user, ct);
            ThrowIfFailed(result, "Kullanıcı güncellenemedi");
            await InvalidateUserCacheAsync(id, ct);
            return user.ToResponseDto();
        }

        public async Task DeleteAsync(Guid id ,Guid currentUserId,CancellationToken ct)
        {
            if (id == currentUserId)
                throw new ForbiddenException("Kendi hesabınızı silemezsiniz.");

            var user = await GetUserOrThrowAsync(id, ct);
            await EnsureCanManageAsync(user, currentUserId, ct);
            var result = await _userRepository.DeleteAsync(user,ct);

            // Silme hatası istemcinin girdisinden kaynaklanmaz (genelde eşzamanlılık/DB hatası) → 500
            if (!result.Succeeded)
                throw new InvalidOperationException($"Kullanıcı silinemedi ({id}): {JoinErrors(result)}");
            // Silme başarılı: hem tek kaydı hem listeleri cache'ten temizle
            await InvalidateUserCacheAsync(id, ct);
        }

        public async Task<LoginResponseDto> ChangeUserNameAsync(Guid userId, ChangeUserNameRequestDto changeUserNameRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(changeUserNameRequestDto, ct);
            var user = await GetUserOrThrowAsync(userId, ct);

            if (!await _userRepository.CheckPasswordAsync(user, changeUserNameRequestDto.CurrentPassword, ct))
                throw new ValidationException("Mevcut parola yanlış.");

            // Değiştirmeden ÖNCE saklıyoruz çünkü notification da eski kullanıcı adının ne olduğuda önemli
            var oldUserName = user.UserName;

            var result = await _userRepository.SetUserNameAsync(user, changeUserNameRequestDto.NewUserName, ct);
            ThrowIfFailed(result, "Kullanıcı adı değiştirilemedi");
            // UserName DTO'da görünüyor: tek kaydı ve listeleri temizler
            await InvalidateUserCacheAsync(userId, ct);
            // Değişiklik kaydedildi → kullanıcıya ve adminlere haber ver
            await _notificationService.NotifyUserNameChangedAsync(
                new UserNameChangedDto
                (
                      userId,
                      oldUserName ?? String.Empty,
                      changeUserNameRequestDto.NewUserName,
                      DateTime.UtcNow), ct);

            // SetUserNameAsync stamp'i yeniledi, eski token artık reddedilecek.
            // Kullanıcı oturumdan düşmesin diye yeni ad + yeni stamp ile token üretip dönüyoruz.
            var roles = await _userRepository.GetRolesAsync(user, ct);
            var token = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.UserName!, user.SecurityStamp!, roles);

            return new LoginResponseDto(token.Token, token.ExpiresAt);

        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto changePasswordRequestDto, CancellationToken ct)
        {
            await _validationService.ValidateAsync(changePasswordRequestDto, ct);
            var user = await GetUserOrThrowAsync(userId, ct);

            var result = await _userRepository.ChangePasswordAsync(user, changePasswordRequestDto.CurrentPassword, changePasswordRequestDto.NewPassword, ct);
            ThrowIfFailed(result, "Parola değiştirilemedi");
            await _cache.RemoveAsync(CacheKeys.SecurityStamp(userId), ct); //bu işlem  sadece şifre değiştiği durumda oluyor.


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
            await _cache.RemoveAsync(CacheKeys.SecurityStamp(id), ct);   // eski token'lar hemen reddedilsin
        }


        // Bir admin başka bir admin'in hesabına dokunamaz (güncelleme/silme).
        // Kendi hesabı serbest. Hedef normal kullanıcıysa da serbest.
        private async Task EnsureCanManageAsync(AppUser target, Guid currentUserId, CancellationToken ct)
        {
            if (target.Id == currentUserId)
                return;

            var roles = await _userRepository.GetRolesAsync(target, ct);
            if (roles.Contains(AdminRole))
                throw new ForbiddenException("Başka bir yöneticinin hesabında değişiklik yapamazsınız.");
        }

    }
}