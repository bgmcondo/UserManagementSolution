using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;




namespace UserManagement.API.Controllers
{
    //[ApiController] Model doğrulamasını otomatik yapar (geçersiz veri gelirse 400 döner),
    //bodydeki json'ı parametreye otomatik bağlar
    //[Route]: Adres "api/auth" olur ([controller] = sınıf adından "Controller" atılmış hali).
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase  // ControllerBase: View'sız (sadece API) controller'ın temeli
    {
        //controller işi iş mantığı değil isteği alır servisi çağırır ve cevap döner

        private readonly IAuthService _authService;

        // IAuthService'i DI verir (AddApplication'da kaydedildi)
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //POST api/auth/register yeni kullanıcı kaydı
        [HttpPost("Login")]
        [AllowAnonymous]   // Giriş yapmadan erişilebilir (zaten giriş yapmak için geliyor)
        public async Task<IActionResult>Login(LoginRequestDto loginRequestDto,CancellationToken ct)
        {
            var result = await _authService.LoginAsync(loginRequestDto,ct);
            return Ok(result);

        }

    
    }
}
