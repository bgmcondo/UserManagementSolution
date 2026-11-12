using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces
{
    // IdentityResult dönen metotlar hata FIRLATMAZ, sonuç nesnesi döner:
    //   result.Succeeded → işlem başarılı mı
    //   result.Errors    → başarısızsa nedenleri
    // Kullanıcının bulunup bulunmadığını servis kontrol eder; bu metotlar bulunmuş bir AppUser alır.
    public interface IUserRepository
    {
        Task<List<AppUser>> GetAllAsync(CancellationToken ct);

        // Id ile kullanıcıyı bulur. Bulunamazsa null döner.
        Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct);

        // E-posta ile kullanıcıyı bulur. Bulunamazsa null döner.
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct);

        // Kullanıcı adı ile bulur (login'de "@" içermeyen değer için). Bulunamazsa null döner.
        Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken ct);

        // E-posta kayıtlı mı?
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

        // Kullanıcıyı oluşturur ve rol atar. Rol atanamazsa kullanıcı geri silinir.
        Task<IdentityResult> CreateAsync(AppUser user, string password, string role, CancellationToken ct);

        Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken ct);

        Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken ct);

        // UserManager.SetUserNameAsync kullanır.
        Task<IdentityResult> SetUserNameAsync(AppUser user, string newUserName, CancellationToken ct);

        // Mevcut parolayı doğrular, yenisini ayarlar.
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword, CancellationToken ct);

        Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct);

        // Rol adlarını döner.
        Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken ct);
    }
}