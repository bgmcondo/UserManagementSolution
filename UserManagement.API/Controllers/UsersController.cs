using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.API.Controllers
{
    [ApiController]//Bu bir API controller'ı, doğrulamayı otomatik yapar
    [Route("api/[controller]")] //Controllerın adresini belirler
    [Authorize] //Tokensız gelen her istek 401 alıyor.Rol isteyenler ayrıca 403 verebilir
    public class UsersController : ControllerBase
    {
        private const string AdminRole = "Admin";
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;

        }

        //Tokendaki sub claimi JwtBearer tarafından claim identifera çevrilir
        // [Authorize] olduğu için buraya gelen her istekte token var, claim boş olamaz.
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [Authorize(Roles =AdminRole)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _userService.GetAllAsync(ct);
            return Ok(result);

        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _userService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> Create(CreateUserRequestDto createUserRequestDto, CancellationToken ct)
        {
            var result = await _userService.CreateAsync(createUserRequestDto, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> Update(Guid id, UpdateUserRequestDto updateUserRequestDto, CancellationToken ct)
        {
            var result = await _userService.UpdateAsync(id,CurrentUserId, updateUserRequestDto, ct);
            return Ok(result);

        }
        // DELETE api/users/{id} → Kullanıcıyı siler
        // Kullanıcı yoksa servis NotFoundException fırlatır → middleware 404 döner
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id,CurrentUserId ,ct);

            return NoContent(); // 204: Silindi, dönecek veri yok
        }

        // ---------- Giriş yapmış her kullanıcı, sadece KENDİSİ için ----------
        // id URL'den alınmaz, token'dan alınır. Böylece kimse başkasının hesabına dokunamaz.

        // GET api/users/me → Kendi bilgilerim
        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken ct)
        {
            var result = await _userService.GetByIdAsync(CurrentUserId, ct);
            return Ok(result);
        }

        // PATCH api/users/username → Kullanıcının sadece kullanıcı adını değiştirir
        // id → URL'den, request → body'deki JSON'dan gelir ([ApiController] otomatik ayırır)
        // Kullanıcı yoksa servis KeyNotFoundException fırlatır → middleware 404 döner
        [HttpPatch("me/username")]

        public async Task<IActionResult> ChangeUserName(ChangeUserNameRequestDto changeUserNameRequestDto, CancellationToken ct)
        {
            var result = await _userService.ChangeUserNameAsync(CurrentUserId, changeUserNameRequestDto, ct);
            return Ok(result);  //yeni token böylece dönüyor

        }
        [HttpPatch("me/password")]
        public async Task<IActionResult> ChangeUserPassword( ChangePasswordRequestDto changePasswordRequestDto, CancellationToken ct)
        {
            await _userService.ChangePasswordAsync(CurrentUserId, changePasswordRequestDto, ct);
            return NoContent();   // 204: Güncellendi, dönecek veri yok

        }
    }
}
