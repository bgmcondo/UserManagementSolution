using UserManagement.Application.DTOs;

namespace UserManagement.Application.Interfaces

//Adminlere canlı bildirim gönderme sözleşmes,.
//Implementasyonu API/Notifications/SignalRNotificationService.cs.
//
//Neden implementasyon API'de, Infrastructure'da değil?
//SignalR, ASP.NET Core'un web katmanına ait bir teknoloji.Hub sınıfı (NotificationHub)
// ve bağlantılar API projesinde yaşıyor .Implemantasyonunda hub'a erişebilmesi için orada olmalı.
//
//Neden Arayüz?
//UserService bildiriminin SignalR ile mi, e-postayla mı, başka bir yolla mı gittiğini bilmemeli.
//
//bildirimler sadece o anda bağlı olan adminlere gider.Bağlı değilse bildirim kaybolur
//sonradan iletilmez.Belgede istenen davranış bu ("o anda bağlı olan tüm Admin rolündeki kullanıcılara").
{
    public interface INotificationService
    {
        // Yeni kullanıcı eklendiğinde Admin'lere bildirim gönderir.
        // UserService.CreateAsync içinde, kullanıcı kaydedildikten sonra çağrılır.
        Task NotifyUserCreatedAsync(UserResponseDto user, CancellationToken ct);

        // Bir kullanıcı kendi kullanıcı adını değiştirdiğinde Admin'lere bildirim gönderir.
        // UserService.ChangeUserNameAsync içinde, ad başarıyla değiştikten sonra çağrılır.
        Task NotifyUserNameChangedAsync(UserNameChangedDto notification, CancellationToken ct);
    }
}
