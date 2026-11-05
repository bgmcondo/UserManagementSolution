

namespace UserManagement.Application.DTOs;




/*
 * =====================================================================
 * NEDEN RECORD? (Bu açıklama tüm DTO'lar için geçerlidir)
 * =====================================================================
 *
 * DTO'nun görevi veriyi bir yerden bir yere OLDUĞU GİBİ taşımak.
 * record bu iş için tasarlanmış bir tiptir.
 *
 * 1) KISA YAZIM
 *    public record GeoDto(double Latitude, double Longitude);
 *    Aynısını class ile yazmak; iki property, constructor ve atamalarla
 *    yaklaşık dokuz satır tutar. Parantez içindeki her parametre otomatik
 *    olarak property'ye dönüşür.
 *
 * 2) DEĞİŞTİRİLEMEZLİK (IMMUTABILITY)
 *    Oluşturulduktan sonra değerleri değiştirilemez:
 *        geo.Latitude = 50.0;   → derleme hatası
 *    Aynı UserResponse nesnesi Redis'e yazılıyor, SignalR ile gönderiliyor
 *    ve API'den dönüyor. Bir yerde yanlışlıkla değiştirilse diğer yerler de
 *    bozuk veri alırdı. Record buna izin vermez; taşınan veri yolda değişmez.
 *
 * 3) DEĞERE GÖRE EŞİTLİK
 *        new GeoDto(41, 29) == new GeoDto(41, 29)
 *        → record: true   |   class: false
 *    class'ta == bellekteki aynı nesne mi diye bakar; record'da içindeki
 *    değerlere bakar. Testlerde beklenen DTO ile gelen DTO'yu tek satırda
 *    karşılaştırmayı sağlar:
 *        result.Should().Be(expected);
 *
 * 4) OKUNAKLI ÇIKTI
 *    ToString() içeriği gösterir:
 *        record: GeoDto { Latitude = 41, Longitude = 29 }
 *        class : UserManagement.Application.DTOs.GeoDto
 *    Loglarda ve hata ayıklarken nesnenin içi doğrudan görünür.
 *
 * ---------------------------------------------------------------------
 * PEKİ ENTITY'LER (AppUser) NEDEN RECORD DEĞİL?
 * ---------------------------------------------------------------------
 *                     DTO                  Entity
 *    Görevi           Veriyi taşımak       Bir şeyi temsil etmek
 *    Değişmeli mi?    Hayır                Evet (adres güncellenir)
 *    Eşitlik          Değerlere göre       Kimliğe (Id) göre
 *    Uygun tip        record               class
 *
 *    Adı ve e-postası tesadüfen aynı olan iki kullanıcı, Id'leri farklıysa
 *    farklı kişilerdir. Record'un "değerler aynıysa eşittir" mantığı entity
 *    için yanlış olur. EF Core da entity'lerin değiştirilebilir olmasını ve
 *    referans eşitliğini bekler.
 *
 *    Kural: taşınan veri → record, yaşayan nesne → class
 * =====================================================================
 */

// Coğrafi konum. Domain'deki GeoLocation value object'inin DTO karşılığı.
// Enlem -90 ile 90, boylam -180 ile 180 arasında olmalı (AddressDtoValidator kontrol eder).
public record GeoDto(double Latitude, double Longitude);