# UserManagement.Infrastructure

**Sorusu: "Nasıl yapılacak?"** Application'daki arayüzlerin gerçek, teknolojiye bağlı implementasyonları.

Application'a referans verir (dolaylı olarak Domain'e de).

## Klasörler

| Klasör | İçerik | Uyguladığı arayüz / görevi |
|---|---|---|
| `Persistence` | `AppDbContext`, `Migrations/` | C# sınıflarının tablolara nasıl dönüşeceği |
| `Repositories` | `UserRepository` | `IUserRepository` (UserManager + EF Core) |
| `Caching` | `RedisCacheService` | `ICacheService` (Redis) |
| `Security` | `JwtTokenService`, `JwtOptions` | `ITokenService` (JWT üretimi) |
| `Email` | `SmtpEmailSender`, `SmtpOptions` | `IEmailSender` (SMTP / Mailpit) |
| `Messaging` | `UserCreatedConsumer` | RabbitMQ'dan mesajı alıp e-posta gönderir |
| `Seeding` | `DataSeeder`, `JsonPlaceholderUser`, `SeedOptions` | Açılışta migration + örnek veri |
| (kök) | `DependencyInjection.cs` | "Biri `ICacheService` isterse `RedisCacheService` ver" eşleştirmeleri |

## Tasarım kararları

### AppDbContext

- `IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>`'den türer. Identity tabloları (`AspNetUsers`, `AspNetRoles` ...) otomatik gelir.
- `base.OnModelCreating(builder)` **silinmemeli**, Identity tabloları onunla kurulur.
- **Owned Type (`OwnsOne`):** `Address`, `GeoLocation`, `Company` ayrı tabloda değil, `AspNetUsers`'ta sütun olarak saklanır. `HasColumnName` ile kısa sütun adları verildi (`AddressCity`, `GeoLatitude`...).
- **Unique Index:** Identity e-posta index'ini varsayılan olarak **benzersiz olmayan** kurar. `HasIndex(u => u.NormalizedEmail).IsUnique()` ile veritabanı seviyesinde benzersizlik sağlandı.

### UserRepository

- `UserManager` kullanır. `CreateAsync` parolayı **otomatik hash'ler**.
- `UserManager` metotlarının çoğu `CancellationToken` almaz. Alabilen sorgularda (`ToListAsync`, `FirstOrDefaultAsync`) token iletilir. Bu yüzden `FindByEmailAsync` yerine sorgu elle yazıldı.
- Listeleme `AsNoTracking()` kullanır (salt okunur, daha hızlı).

### Identity ayarları

- `AddIdentity` yerine `AddIdentityCore`: Klasik `AddIdentity` cookie tabanlı oturum da kurar. JWT kullanıldığı için yalnızca kullanıcı yönetimi yeterli.
- Parola kuralları Application'daki validator ile **aynı** olmalı.

### Redis

- `IDistributedCache` + JSON serileştirme.
- `InstanceName = "usermgmt:"` → Redis'teki gerçek anahtar `usermgmt:users:all` olur. Aynı Redis'i paylaşan uygulamalar çakışmaz.
- `AbsoluteExpirationRelativeToNow`: veri en geç 10 dakikada kendiliğinden silinir. Invalidation bir şekilde kaçırılsa bile veri sonsuza kadar eski kalmaz.

### JWT

- Token içinde: `sub` (Id), `email`, `jti` (benzersiz token Id), `name`, `role` claim'leri.
- Rol için `ClaimTypes.Role` kullanılır. `[Authorize(Roles = "Admin")]` bu claim'e bakar.
- İmza: HMAC-SHA256. **Anahtar en az 32 karakter** olmalı, yoksa token üretilirken hata alınır.

### MassTransit + RabbitMQ

- **Sürüm 8.x** kullanılıyor. v9 ticari lisanslı, v8 Apache 2.0 açık kaynak. `MassTransit.Abstractions` ve `MassTransit.RabbitMQ` **birebir aynı sürüm** olmalı.
- `ConfigureEndpoints(ctx)`: kuyruk adları consumer adından otomatik üretilir.
- `UseMessageRetry(3 kez, 5 sn)`: Hata durumunda tekrar denenir. Sonunda başarısız olursa mesaj `_error` kuyruğuna taşınır, kaybolmaz.

### E-posta

- Geliştirmede **Mailpit** kullanılır: gerçek e-posta göndermez, gelen kutusunu http://localhost:8025 adresinde gösterir.
- `SmtpClient` basit ama eski bir API. Gerçek projelerde **MailKit** tercih edilir. Arayüz kullanıldığı için sadece bu sınıf değişir.

### DataSeeder

1. Migration'ları uygular (`Database.MigrateAsync`).
2. `Admin` ve `User` rollerini oluşturur.
3. Veritabanında kullanıcı varsa **durur** (idempotent: her açılışta tekrar eklemez).
4. jsonplaceholder'dan kullanıcıları çeker, `AppUser`'a dönüştürür, varsayılan parolayla kaydeder.
5. İlk kullanıcı (`Sincere@april.biz`) **Admin**, diğerleri **User**.

**Dış model ile entity ayrımı:** JSON, `JsonPlaceholderUser` record'larına okunur, sonra `AppUser`'a dönüştürülür. Dış servisin formatı değişirse yalnızca bu dönüşüm değişir, entity etkilenmez. JSON'daki `id` (1, 2, 3...) kullanılmaz, bizim Id'miz `Guid`.

**Türkçe Windows tuzağı:** Ondalık ayırıcı virgüldür. `double.Parse("-37.3159")` hata verir veya yanlış sayı üretir. Bu yüzden **`CultureInfo.InvariantCulture`** zorunlu.

### Ayarlar (Options Pattern)

`JwtOptions`, `SmtpOptions`, `SeedOptions` config bölümlerine bağlanır (`services.Configure<T>`). Değerler koda yazılmaz.

### Sürüm notları (.NET 10)

| Paket grubu | Sürüm |
|---|---|
| Microsoft paketleri (EF Core, Identity, Redis cache, Http) | 10.x (.NET sürümüyle aynı) |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.x (EF Core ile aynı ana sürüm) |
| MassTransit.RabbitMQ | 8.x |
| System.IdentityModel.Tokens.Jwt | 8.x (.NET'ten bağımsız numaralandırma) |

Class library'de bazı Microsoft paketleri NU1510 ("gereksiz paket") uyarısı verebilir. Derlemeyi engellemez.

## Migration

Paket Yöneticisi Konsolu:

```powershell
Add-Migration <Ad> -Project UserManagement.Infrastructure -StartupProject UserManagement.API -OutputDir Persistence/Migrations
```

`Update-Database` gerekmez. `DataSeeder` açılışta otomatik uygular.