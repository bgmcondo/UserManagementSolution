using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories
{
    // Kullanıcı verisine erişimi Identity'nin UserManager'ı üzerinden yapar.
    // Şifre hash'leme, doğrulama ve rol yönetimi Identity tarafından yürütülür.
    //
    // Not: UserManager metotlarının çoğu CancellationToken almaz.
    // O yüzden en azından işe başlamadan önce iptal edilip edilmediğini kontrol ediyoruz.
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(UserManager<AppUser>userManager)
        {
            _userManager = userManager;
        }
        public Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        }

        public Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, string password, string role, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            //kullanıcı oluştur
            var createdResult = await _userManager.CreateAsync(user, password);
            //oluşturulmadıysa hatayı geri dön
            if (!createdResult.Succeeded)
                return createdResult;
            // Kullanıcıya rolü ata
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            //rol atanmadıysa kullanıcıya kullanıcının silinmesi şart
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return roleResult;
            }
            return createdResult;
        }

        public Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.DeleteAsync(user);
        }

        public Task<List<AppUser>> GetAllAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return  _userManager.Users.AsNoTracking().ToListAsync(ct);
        }

        public Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.FindByEmailAsync(email);

        }

        public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.FindByIdAsync(id.ToString());
        }

        public Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.FindByNameAsync(userName);
        }

        public Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.GetRolesAsync(user);

        }

        public Task<IdentityResult> SetUserNameAsync(AppUser user, string newUserName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.SetUserNameAsync(user, newUserName);
        }

        public Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return _userManager.UpdateAsync(user);
        }
        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            // Identity e-postaları büyük harfe çevrilmiş olarak NormalizedEmail alanında saklar.
            var normalizedEmail = _userManager.NormalizeEmail(email);

            return _userManager.Users
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail, ct);
        }

    }
}
