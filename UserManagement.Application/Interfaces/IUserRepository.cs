
using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Entities;


namespace UserManagement.Application.Interfaces
{
    // Kullanıcı veritabanı işlemleri için arayüz sözleşmesi. Bu arayüz, kullanıcı verilerini yönetmek için gerekli olan temel işlemleri tanımlar. Örneğin, kullanıcı ekleme, silme, güncelleme ve sorgulama gibi işlemler bu arayüzde tanımlanabilir.

    //CancellationToken, işlemin iptal edilip edilmeyeceğini kontrol etmek için kullanılır. Bu sayede uzun süren işlemler sırasında kullanıcı isteği iptal edebilir ve kaynaklar gereksiz yere kullanılmaz.

    // IdentityResult dönen metotlar hata FIRLATMAZ, sonuç nesnesi döner:
    //   result.Succeeded → işlem başarılı mı
    //   result.Errors    → başarısızsa nedenleri (DuplicateUserName, PasswordMismatch vb.)
    // Kullanıcının bulunup bulunmadığını servis kontrol eder, bu metotlar bulunmuş bir AppUser alır.
    public interface IUserRepository
    {

        Task<List<AppUser>> GetAllAsync(CancellationToken ct);

        //Bu metot , kullanıcı adını veya e-posta adresini kullanarak kullanıcıyı bulur. Kullanıcı bulunamazsa null döner.
        Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct);


        // Bu metot, kullanıcı adını kullanarak kullanıcıyı bulur. Kullanıcı bulunamazsa null döner.
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct);

        // Login'de "@" içermeyen değerle arama için
        Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken ct);

        // Bu metot, yeni bir kullanıcı ekler. Kullanıcı zaten varsa hata fırlatır.
        Task<IdentityResult> CreateAsync(AppUser user, string password,string role, CancellationToken ct);

        // Bu metot, mevcut bir kullanıcıyı günceller. Kullanıcı bulunamazsa hata fırlatır.
        Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken ct);

        // Bu metot, mevcut bir kullanıcıyı siler. Kullanıcı bulunamazsa hata fırlatır.
        Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken ct);

        //Kullanıcı adı değişikliği için.UserManager.SetUserNameAsync() kullanılır. Bu metot, kullanıcı adını günceller ve IdentityResult döner. Kullanıcı bulunamazsa hata fırlatır.
        Task<IdentityResult> SetUserNameAsync(AppUser user, string newUserName, CancellationToken ct);

        //Parola değişikliği için. UserManager.ChangePasswordAsync() kullanılır. Bu metot, mevcut parolayı doğrular ve yeni parolayı ayarlar. IdentityResult döner. Kullanıcı bulunamazsa hata fırlatır.

        Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword,string newPassword,CancellationToken ct);

        // Parola kontrol 
        Task<bool> CheckPasswordAsync(AppUser user, string password,CancellationToken ct);

        //Rollerin listesi ama rol adlarını listeledği için string dönmesi lazım

        Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken ct);

    }
}
