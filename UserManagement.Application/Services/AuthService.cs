using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Common;
namespace UserManagement.Application.Services
{
 //Giriş akışını yönetir
 //1.kullacıyı bul
 //Şifreyi doğrula
 //rolleri al
 //token üret

    public class AuthService : IAuthService
    
    {
        private readonly IUserRepository _userRepository;
    
        private readonly IJwtTokenService _tokenService;
        // Kullanıcı yok ve parola yanlış için ortak mesaj.
        // Farklı mesaj verilirse saldırgan hangi hesapların kayıtlı olduğunu öğrenebilir.

        private readonly IValidationService _validationService;
        private const string InvalidCredentialsMessage = "Kullanıcı adı/e-posta veya parola hatalı.";
        private const string LockedOutMessage = "Çok fazla hatalı deneme. Hesabınız geçici olarak kilitlendi, daha sonra tekrar deneyin.";
        public AuthService(

       IValidationService validationService,
       IUserRepository userRepository,

       IJwtTokenService tokenService)
        {
            _validationService = validationService;

            _userRepository = userRepository;
           
            _tokenService = tokenService;
        }
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct)
        {
            // 1) Boş/geçersiz girdi → 400 (veritabanına hiç gitmeden)
            await _validationService.ValidateAsync(request, ct);

            //kullanıcıyı eposta adresiyle buluyoruz
            var user = await _userRepository.GetByEmailAsync(request.UserNameOrEmail, ct) ?? await
                _userRepository.GetByUserNameAsync(request.UserNameOrEmail,ct);
            //kullanıcı veya şifre yoksa null dönüyoruz
            //    İkisine aynı cevabı veriyoruz; farklı mesaj verirsek saldırgan
            //    hangi e-postaların sistemde kayıtlı olduğunu öğrenebilir.
            if (user == null)
                throw new UnauthorizedException(InvalidCredentialsMessage);

            var passwordResult = await _userRepository.CheckPasswordWithLockoutAsync(user, request.Password, ct);

            if (passwordResult == PasswordCheckResult.LockedOut)
                throw new UnauthorizedException(LockedOutMessage);

            if (passwordResult == PasswordCheckResult.Failed)
                throw new UnauthorizedException(InvalidCredentialsMessage);
            //roller appuser de yok repositoryden alıyoruz
            var roles = await _userRepository.GetRolesAsync(user,ct);
            if (!roles.Any())
            {
                throw new ForbiddenException("Hesabınıza rol atanmamış. Yöneticiyle iletişime geçin.");
            }
            // 4) Token'ları üret (bellekte hesaplama, await gerekmez)
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!,user.SecurityStamp! ,roles);

            return new LoginResponseDto(accessToken.Token, accessToken.ExpiresAt);
        }
    }
}
