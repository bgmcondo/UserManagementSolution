# UserManagement.Application

**Sorusu: "Ne yapılacak, hangi sırayla?"** İş mantığının yaşadığı katman.

Yalnızca **Domain**'e referans verir. Redis, RabbitMQ, SignalR, PostgreSQL gibi teknolojileri **tanımaz**, sadece onların **arayüzlerini** bilir.

## Klasörler

| Klasör | İçerik | Mantığı |
|---|---|---|
| `DTOs` | `UserDtos.cs`, `AuthDtos.cs` | Dış dünyayla konuşma formatı. Entity doğrudan dışarı verilmez (yoksa `PasswordHash` sızar). |
| `Events` | `UserCreatedEvent` | RabbitMQ'ya gönderilecek "kullanıcı oluşturuldu" mesajının sözleşmesi. |
| `Interfaces` | `IUserRepository`, `ICacheService`, `IEmailSender`, `INotificationService`, `ITokenService`, `IUserService`, `IAuthService` | Diğer katmanların uygulayacağı sözleşmeler. |
| `Common` | `CacheKeys` | Birden fazla yerde kullanılan sabitler. Interface değildir, static class'tır. |
| `Mappings` | `UserMappings` | Entity ↔ DTO dönüşümleri (extension method). |
| `Services` | `UserService`, `AuthService` | Asıl iş akışı. |
| `Validators` | `CreateUserRequestValidator` vb. | FluentValidation kuralları. |
| (kök) | `DependencyInjection.cs` | Bu katmanın servis kayıtları (`AddApplication()`). |

## Temel fikir: Dependency Inversion (SOLID - D)

`UserService` şunları **bilmez**: Redis, RabbitMQ, SignalR, EF Core.
Şunları **bilir**: `ICacheService`, `IPublishEndpoint`, `INotificationService`, `IUserRepository`.

Sonuçları:
- Redis yerine başka bir cache kullanılırsa `UserService` değişmez.
- Unit testlerde bu arayüzler **mock** nesnelerle değiştirilir. Test için gerçek Redis veya veritabanı gerekmez.

## Tasarım kararları

### Neden `IUserRepository` var? `UserManager` doğrudan kullanılamaz mıydı?

`UserManager`'ı mock'lamak zahmetlidir (çok sayıda constructor parametresi). Repository arayüzü sayesinde servis testleri basit kalır. `UserManager` kullanan gerçek implementasyon Infrastructure'dadır.

### Cache-aside deseni (okuma)

```
1. Redis'e bak → varsa döndür
2. Yoksa veritabanından al
3. Redis'e yaz (10 dakika geçerli)
4. Döndür
```

### Cache invalidation (yazma)

Ekleme, güncelleme ve silme işlemlerinden sonra ilgili anahtarlar silinir:
- `users:all` → liste değişti
- `users:{id}` → o kullanıcının detayı değişti

Bir sonraki okumada güncel veri tekrar cache'e girer. Anahtarlar `CacheKeys` içinde tek yerde tutulur, yazım hatası riski olmaz.

### Hata yönetimi

| Durum | Servis ne yapar | API ne döner |
|---|---|---|
| Geçersiz veri | `ValidationException` fırlatır | 400 |
| E-posta zaten kayıtlı | `ValidationException` fırlatır | 400 |
| Identity hatası (zayıf parola vb.) | `ValidationException` fırlatır | 400 |
| Kayıt bulunamadı | `null` veya `false` döner | 404 |
| Hatalı giriş bilgisi | `null` döner | 401 |

Login'de "e-posta yanlış" ile "parola yanlış" ayırt edilmez. Ayırt edilirse saldırgan hangi e-postaların sistemde kayıtlı olduğunu öğrenebilir.

### Validator ve Identity kuralları uyumlu olmalı

Parola kuralları (8+ karakter, büyük/küçük harf, rakam, özel karakter) hem `CreateUserRequestValidator`'da hem Infrastructure'daki Identity ayarlarında aynıdır. Uyumsuz olursa validator kabul edip Identity reddeder ve kullanıcı tutarsız hatalar görür.

### İç içe nesnelerin doğrulanması

`Address` ve `Company` için ayrı validator'lar yazıldı ve `SetValidator` ile ana validator'a bağlandı. Alan `null` ise doğrulama atlanır (`When(x => x.Address is not null, ...)`).

### Mapping neden elle yapılıyor?

AutoMapper ticari lisansa geçti. Bu boyutta bir projede extension method'lar yeterli, daha okunaklı ve hata ayıklaması kolay.

### Event yayınlama sırası

```
Kaydet → Cache temizle → RabbitMQ'ya event bırak → SignalR bildirimi
```

**Bilinen sınırlama:** Veritabanı kaydı ile RabbitMQ mesajı iki ayrı işlemdir. Arada RabbitMQ çökerse kullanıcı oluşur ama e-posta gitmez. Kurumsal çözümü **Transactional Outbox** desenidir (MassTransit destekler).