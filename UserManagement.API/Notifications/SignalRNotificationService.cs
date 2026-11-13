using Microsoft.AspNetCore.SignalR;
using UserManagement.API.Hubs;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.API.Notifications
{
    //Notification servisinin signalR ile uygualaması
    //api de duruyor çünkü hub ve soket bağlantıları web sunucusunda yaşar
    //application sadece NotificationService'i bilir, SignalR'dan habersizdir.
    
    public class SignalRNotificationService:INotificationService
    {
        // Frontend'in dinlediği olay adı. Frontend: connection.on("ReceiveNotification", ...)
        private const string ReceiveMethod = "ReceiveNotification";

        //Hub'ın dışından (servis,consumer) bağlı istemcilere mesaj göndermeyi sağlar
        //hub nesnesini doğrudan kullanamayız,çünkü hub sadece bir bağlantı süresince yaşar

        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            
        }

    

        public Task NotifyUserCreatedAsync(UserResponseDto user, CancellationToken ct)
        {
            var message = $"Yeni kullanıcı oluşturuldu: {user.FullName} ({user.Email})";
            return SendToAdminsAsync(message, ct);
        }

        public Task NotifyUserNameChangedAsync(UserNameChangedDto notification, CancellationToken ct)
        {
            var userMessage = $"kullanıcı adınız '{notification.NewUserName}' olarak değiştirildi";
            var adminMessage =$"'{notification.OldUserName}' kullanıcısının kullanıcı adı'{notification.NewUserName}' olarak değiştirildi";
            // İki gönderimi aynı anda başlatır, ikisi de bitince tamamlanır.
            return Task.WhenAll(
                SendToUserAsync(notification.UserId, userMessage, ct),
                SendToAdminsAsync(adminMessage, ct));
        }
        
          
        //Belirli bir kullanıcya gönderir
        // SignalR kullanıcıyı token'daki NameIdentifier claim'inden tanır → JWT kurulmadan çalışmaz.
        private  Task SendToUserAsync(Guid userId, string message, CancellationToken ct)
        {
            return _hubContext.Clients.User(userId.ToString()).SendAsync(ReceiveMethod, message, ct);
        }

        //NotificationHub.OnConnectedAsync()'te "Admins" grubuna eklenen herkese gönderir.
         private Task SendToAdminsAsync(string message, CancellationToken ct = default)
        {
            return _hubContext.Clients
                  .Group(NotificationHub.AdminsGroup)
                  .SendAsync(ReceiveMethod, message, ct);

        }
    }
}
