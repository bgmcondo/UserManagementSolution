using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Validators;

namespace UserManagement.API.Controllers
{
    [ApiController]//Bu bir API controller'ı, doğrulamayı otomatik yapar
    [Route("api/[controller]")] //Controllerın adresini belirler
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _userService.GetAllAsync(ct);
            return Ok(result);

        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _userService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequestDto createUserRequestDto, CancellationToken ct)
        {
            var result = await _userService.CreateAsync(createUserRequestDto, ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserRequestDto updateUserRequestDto, CancellationToken ct)
        {
            var result = await _userService.UpdateAsync(id, updateUserRequestDto, ct);
            return Ok(result);

        }
        // DELETE api/users/{id} → Kullanıcıyı siler
        // Kullanıcı yoksa servis KeyNotFoundException fırlatır → middleware 404 döner
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id, ct);

            return NoContent(); // 204: Silindi, dönecek veri yok
        }
        // PATCH api/users/{id}/username → Kullanıcının sadece kullanıcı adını değiştirir
        // id → URL'den, request → body'deki JSON'dan gelir ([ApiController] otomatik ayırır)
        // Kullanıcı yoksa servis KeyNotFoundException fırlatır → middleware 404 döner
        [HttpPatch("{id:guid}/username")]

        public async Task<IActionResult> ChangeUserName(Guid id, ChangeUserNameRequestDto changeUserNameRequestDto, CancellationToken ct)
        {
            await _userService.ChangeUserNameAsync(id, changeUserNameRequestDto, ct);
            return NoContent();   // 204: Güncellendi, dönecek veri yok

        }
        [HttpPatch("{id:guid}/password")]
        public async Task<IActionResult> ChangeUserPassword(Guid id, ChangePasswordRequestDto changePasswordRequestDto, CancellationToken ct)
        {
            await _userService.ChangePasswordAsync(id, changePasswordRequestDto, ct);
            return NoContent();   // 204: Güncellendi, dönecek veri yok

        }
    }
}
