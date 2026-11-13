using Microsoft.AspNetCore.SignalR;

namespace UserManagement.API.Hubs
{
    // SignalR hub'ı: Frontend bu sınıfa bağlanır, sunucu buradan anlık bildirim gönderir.
    // Hub'dan türemek ZORUNLU, yoksa MapHub kabul etmez.
    // [Authorize] // JWT kurulunca açılacak: sadece giriş yapanlar bağlanabilsin


    public class NotificationHub : Hub
    {
        //Bir kullanıcı bağlandığında otomatik çalışır

        // Admin grubunun adı. SignalRNotificationService de bu sabiti kullanır,
        // böylece iki yerde "Admins" yazıp yazım hatası yapma riski kalmaz.
        public const string AdminsGroup = "Admins";
        public override async Task OnConnectedAsync()
        {
            // Context.User nereden gelir?
            // Frontend hub'a token ile bağlanır (?access_token=...) → UseAuthentication token'ı çözer
            // → HttpContext.User dolar → SignalR bunu Context.User'a kopyalar (bağlantı boyunca geçerli).
            // Not: JWT kurulmadan Context.User boş (anonim) olur.

            //admin rolündeki kullanıcıları "AdminsGroup" grubuna ekliyoruz böylece  sadece adminlere bildirim gidebilir
            if (Context.User?.IsInRole("Admin") == true)
                await Groups.AddToGroupAsync(Context.ConnectionId,AdminsGroup);
            await base.OnConnectedAsync();

        }

    }
}
