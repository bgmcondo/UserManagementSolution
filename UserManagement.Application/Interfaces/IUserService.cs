using UserManagement.Application.DTOs;

namespace UserManagement.Application.Interfaces
{
    //Kullanıcı işlemlerinin iş mantığını tanımlar. (Kullanıcı oluşturma, kullanıcı adını değiştirme, kullanıcıyı silme vb.)
    // Implementasyonu Application/Services/UserService.cs.
    // Controller bu arayüzü kullanır, UserService'i doğrudan bilmez. Böylece controller testlerinde mock'lanabilir.

    //
    // IUserRepository'den farkı:
    //   Repository → veriyi okur/yazar, AppUser (entity) ile çalışır.
    //   Service    → iş kurallarını uygular (doğrulama, cache, bildirim, event), DTO ile çalışır.
    //                Dışarıya hiçbir zaman entity vermez.
    //
    //Hata davranışı:
    // -Kullanıcı bulunamazsa null veya false döner,controller 404'e çevirir
    // -Girdi geçersizse veya Identity reddederse ValidationException fırlatılır,400'e çevrilir.

    public interface IUserService
    {

        // Tüm kullanıcıları getirir. Önce Redis'e bakar, yoksa veritabanından alıp cache'ler.
        Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct);

        //Id ile kullanıcıyı getirir.Önce Redis'e sonra bulunamazsa veritabanından alıp cache'ler.
        Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct);

        //Yeni kullanıcı ekler (Admin).Rolü "User" olarak atar,RabbitMQ'ya UserCreatedEvent gönderilir, Admin'lere SignalR bildirimi gider.
        //Sonrasında cache temizlenir
        Task<UserResponseDto> CreateAsync(CreateUserRequestDto createUserRequestDto, CancellationToken ct);

        //Kullanıcı profilini günceller.İki endpoint ortak kullanılır:
        //PUT /api/users/{id} Admin id URL'den
        //PUT /api/users/me kullanıcının kendisi,id token'dan
        //Bulunamazsa false döner
        Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken ct);

        // Kullanıcıyı siler (Admin). Bulunamazsa false döner.
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);

        // Kullanıcının kendi kullanıcı adını değiştirir. userId token'dan gelir, istemciden değil.
        // Mevcut parola yanlışsa veya yeni ad başkasındaysa ValidationException fırlatır.
        // Başarılıysa cache temizlenir ve Admin'lere SignalR bildirimi gider.
        Task<bool> ChangeUserNameAsync(Guid userId, ChangeUserNameRequestDto request, CancellationToken ct);

        // Kullanıcının kendi parolasını değiştirir. userId token'dan gelir.
        // Mevcut parola yanlışsa ValidationException fırlatır.
        // Cache temizliği ve bildirim yok: parola cache'te bulunmuyor, Admin'in bilmesi de gerekmiyor.
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct);



    }
}
