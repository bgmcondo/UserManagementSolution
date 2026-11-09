
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Email
   
//IEmailserver'ın smtp ile çalışan versiyonu 
//mailkit kütüphanesini kullanıyor
//Geliştirmede mailpit 

{
    public class SmtpEmailSender : IEmailSender
    {
        /// SMTP ayarları (Host, Port, kullanıcı adı, şifre, gönderen,gönderilen).
       private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions>options)
        {
            _options = options.Value;
            
        }
        public async Task SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            //epostanın içinin nasıl düzenleneceğini belirler mimemessage yani eposta
            var message = new MimeMessage();
            //kimden gönderiliyor
            message.From.Add(new MailboxAddress(_options.FromName,_options.From));
            // Kime: metin adresten MailboxAddress üret
            message.To.Add(MailboxAddress.Parse(to));
            //eposta konusu
            message.Subject=subject;
            //eposta içeriği
            message.Body = new BodyBuilder {HtmlBody = body}.ToMessageBody();
            //postaneye bağlan
            // "using var" metot bitince bağlantı nesnesi otomatik temizlenir(Dispose)
            using var client = new SmtpClient(); //Epostayı teslim eden kurye gibi düşünülmeli.Uygulamanın SMTP sunucuyla konuşan temsilcisi

            //mailpitte şifreleme yok, Gerçek sunucu: mailkit porta göre kendi seçer
            var security = _options.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;
            await client.ConnectAsync(_options.Host,_options.Port,security,ct);
             
           if(!string.IsNullOrEmpty(_options.UserName))
                await  client.AuthenticateAsync(_options.UserName, _options.Password!,ct);

           // hazırladığımız mimemessage'ı sunucuya gönderiyoruz
           await client.SendAsync(message);
            //  bağlantıyı düzgünce kapat 
            // true → sunucuya "çıkıyorum" (QUIT) mesajı gönder, bağlantıyı aniden koparma
            await client.DisconnectAsync(true, ct);

        }
    }
}
