using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Infrastructure.Email
{
    ///SMTP sunucusuna bağlanmak ve eposta göndermek için gereken ayarlar

    /// <summary>
    /// Neden ayrı bir sınıf?
    /// Sunucu adresi,şifre gibi bilgiler koda yazılmaz;ortama göre değişir
    ///  Geliştirmede Mailpit (localhost:1025), canlıda gerçek SMTP sunucusu kullanılır.
    /// Sadece appsetting değişir geri kalan her şey aynı
    /// property isimleri appsettingjson daki isimler ile birebir aynı olur
     /// Eşleştirme isimlere göre yapılır; yanlış yazılan alan hata vermez, sessizce boş kalır.
    /// </summary>
    public class SmtpOptions
    {
        ///appsetting.jsondaki bölümün adı
        ///DI kaydında "smtp" yazısını tekrar yazmamak için burada sabit olarak tanımlıyoruz
        /// (RedisCacheService.InstanceName ile aynı mantık).
        /// Kullanım: configuration.GetSection(SmtpOptions.SectionName)
        public const string SectionName = "Smtp";

        /// sunucu bilgileri hangi postaneye gidiyorum 
        /// Smtp sunucusunun adresi

        public string Host { get; set; } = string.Empty;

        //Smtp sunucusunun portu
        public int Port { get; set; }

        //Smtp sunucusuna giriş için kullanıcı adı genellikle eposta
        public string? UserName { get; set; }

        //Smtp sunucusuna giriş şifresi
        public string? Password { get; set; }


        //gönderinin eposta adresi
        public string From { get; set; }=string.Empty;

        //göndericinin adı
        public string FromName { get; set; }= string.Empty;

        //Alıcının eposta adresi
       public string To { get; set; }= string.Empty;

        public string ToName { get; set; } = string.Empty;

        ///bağlantı şifrelensin mi
        ///eğer mailpit ise false
        /// true eğer gerçek sunucuda ise
        /// bool varsayılan false;appsettingjson da yazılmazsa şifreleme kapalı olur
     
        public bool UseSsl { get; set; }
      



    }
}
