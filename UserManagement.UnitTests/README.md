# UserManagement.UnitTests

**Sorusu: "Doğru çalışıyor mu?"** Servis, validator ve controller'ların izole birim testleri.

Application ve API projelerine referans verir.

## Kullanılan kütüphaneler

| Kütüphane | Görevi | Not |
|---|---|---|
| **xUnit** | Test çatısı (`[Fact]`, `[Theory]`) | Şablonla gelir |
| **Moq** | Bağımlılıkların sahte (mock) versiyonları | Resmi paket: Daniel Cazzulino / kzu |
| **FluentAssertions** | Okunaklı doğrulamalar (`result.Should().NotBeNull()`) | **7.x** kullanılıyor; 8+ ticari lisanslı |

## Neden unit test bu kadar kolay?

Application katmanındaki servisler (`UserService`, `AuthService`) somut sınıflardır. Ancak **bağımlılıklarını** somut sınıflar olarak değil **arayüzler** olarak alırlar. Örneğin `UserService`, constructor'ında `RedisCacheService` değil `ICacheService` ister. Testte bu arayüzlere sahte (mock) nesneler verilir:

| Gerçekte | Testte |
|---|---|
| `RedisCacheService` | `Mock<ICacheService>` |
| `UserRepository` (PostgreSQL) | `Mock<IUserRepository>` |
| RabbitMQ | `Mock<IPublishEndpoint>` |
| SignalR | `Mock<INotificationService>` |

Böylece test için gerçek Redis, veritabanı veya RabbitMQ çalıştırmak gerekmez. Testler milisaniyeler içinde biter. Dependency Inversion'ın en somut faydası budur.

## Test edilecek senaryolar

### UserService

- **Create:** Geçerli istekte kullanıcı oluşur, `users:all` cache'i temizlenir, `UserCreatedEvent` **bir kez** yayınlanır, adminlere bildirim gider.
- **Create:** Aynı e-posta varsa `ValidationException` fırlar ve event **yayınlanmaz**.
- **GetById:** Cache'te veri varsa repository **hiç çağrılmaz** (`Times.Never`).
- **GetById:** Cache boşsa repository'den alınır ve cache'e yazılır.
- **Update / Delete:** Kullanıcı yoksa `false` döner. Varsa her iki cache anahtarı temizlenir.

### Validator'lar

FluentValidation'ın `TestValidate` yardımcısı kullanılır:
- Geçersiz e-posta, boş ad, zayıf parola → hata
- Geçersiz koordinat (enlem > 90) → hata
- Geçerli istek → hata yok

### Controller'lar

- Servis `null` dönerse → `NotFoundResult` (404)
- Create başarılıysa → `CreatedAtActionResult` (201)
- Update/Delete başarılıysa → `NoContentResult` (204)

## Test yazım kalıbı: Arrange - Act - Assert

```csharp
[Fact]
public async Task MetotAdi_Durum_BeklenenSonuc()
{
    // Arrange: mock'ları ve girdiyi hazırla
    // Act: test edilen metodu çağır
    // Assert: sonucu ve mock çağrılarını doğrula
}
```

Test adları üç parçadan oluşur: **ne test ediliyor**, **hangi durumda**, **ne bekleniyor**. Örnek: `CreateAsync_WhenEmailExists_ThrowsValidationException`.

## TDD (Test-Driven Development) döngüsü

1. **Red:** Önce başarısız olan testi yaz.
2. **Green:** Testi geçirecek en basit kodu yaz.
3. **Refactor:** Testler yeşilken kodu iyileştir.

## Çalıştırma

Visual Studio: **Test → Test Gezgini → Tümünü Çalıştır**

Terminal:

```bash
dotnet test
```