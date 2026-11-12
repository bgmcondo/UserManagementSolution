namespace UserManagement.API.Notifications
{
    //Notification servisinin signalR ile uygualaması
    //api de duruyor çünkü hub ve soket bağlantıları web sunucusunda yaşar
    //application sadece NotificationService'i bilir, SignalR'dan habersizdir.
    
    public class SignalRNotificationService
    {
        // Frontend'in dinlediği olay adı. Frontend: connection.on("ReceiveNotification", ...)
        private const string ReceiveMethod = "ReceiveNotification";

    }
}
