namespace UserManagement.Application.Interfaces
{
    //E-posta gönderme sözleşmesi
    //Implementasyonu Infrastructure/Email/SmtpEmailSender.cs.
    //
    //Kim kullanıyor?
    //UserCreatedConsumer(Infrastructure).RabbitMQ'dan UserCreatedEvent gelince
    //bu arayüz üzerinden hoşgeldin epostası gönderilir 
    //UserService bu arayüzü kullanmaz:e-posta doğrudan değil,kuyruk üzerinden asenkron gider.

    //Neden Bu Arayüz?
    //-Consumer, e-postanın SMTP ile mi ,SendGrid gibi bir servisle mi gittiğini bilmemeli
    //-Yarın sağlayıcı değişirse sadece implementasyon değişir 
    //-Teslerde gerçek e-posta yerine mock kullanılır ve sadece
    //  "SendAsync doğru adres ve konuyla çağrıldı mı?" kontrol edilir.
    //
    //Geliştirme ortamında SMTP sunucusu olarak Mailpit kullanılır: e-postaları gerçekten
    // göndermez, yakalayıp http://localhost:8025 adresinde gösterir.

    //Neden async ve CancellationToken var?
    //ITokenService’in tersine, e-posta göndermek bir I/O işlemi: ağ üzerinden SMTP sunucusuna bağlanıyor.Yavaş olabilir,
    //beklenmesi gerekir.Bu yüzden Task dönüyor ve token alıyor.

    public interface IEmailSender
    {
        // to      → alıcının e-posta adresi
        // subject → konu
        // body    → içerik (HTML olabilir)
        Task SendAsync(string to, string subject, string body, CancellationToken ct);
    }
}
