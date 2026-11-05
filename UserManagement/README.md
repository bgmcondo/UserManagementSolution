# UserManagement.Domain

**Sorusu: "Ne?"** Sistemin temel kavramlarını tanımlar.

Bu katmanda veritabanı, HTTP, Redis, RabbitMQ gibi teknik kavramlar **yoktur**. Hiçbir projeye referans vermez. Mimarinin en iç halkasıdır.

## Klasörler

| Klasör | İçerik |
|---|---|
| `Entities` | `AppUser`: kimliği (Id) olan nesneler |
| `ValueObjects` | `Address`, `GeoLocation`, `Company`: kimliği olmayan, değerleriyle tanımlanan nesneler |

## Entity ve Value Object farkı

**Entity:** Bir kimliği vardır. Değerleri değişse bile aynı şey olarak kalır.
- Kullanıcı e-postasını, telefonunu, adresini değiştirse bile Id'si aynı olduğu için **aynı kullanıcıdır**.

**Value Object:** Kimliği yoktur. Değerleri aynıysa iki nesne aynıdır.
- Aynı sokak, şehir ve posta koduna sahip iki adres **aynı adrestir**. "Adres #47" diye bir kimlikten söz edilmez.
- Benzetme: 100 TL'lik banknot value object'tir, başkasınınkiyle değiştirmek fark etmez. Nüfus cüzdanı ise entity'dir.

## Tasarım kararları

### Address, GeoLocation ve Company neden value object?

- Her kullanıcının **tek** adresi ve şirketi var.
- Bu bilgiler kullanıcıya **ait**. Tek başlarına anlamları yok.
- Kullanıcı silinince bu bilgiler de gitmeli.
- Adres değişiminin geçmişi tutulmuyor.

Bu dört özellik value object tanımına uyar. EF Core'da **Owned Type** (`OwnsOne`) ile ayrı tablo değil, `AspNetUsers` tablosunda sütun olarak saklanırlar.

### Ne zaman entity'ye dönüştürülmeli?

Aşağıdakilerden biri gerekirse adres ayrı bir entity (ayrı tablo, kendi Id'si) olmalıdır:
- Kullanıcı başına **birden fazla** adres (ev, iş, fatura)
- Adresin **tek başına** sorgulanması
- Birden fazla kaydın **aynı** adresi paylaşması
- Adres **geçmişinin** tutulması

### Koordinatlar neden `double`?

Kaynak JSON'da `"-37.3159"` şeklinde metin olarak geliyor. Koordinat sayısal bir veridir, doğru veri tipiyle saklanır. Dönüşüm seeding sırasında `CultureInfo.InvariantCulture` ile yapılır (bkz. Infrastructure notları).

### `AppUser` neden `IdentityUser<Guid>`'den türüyor?

- `Id`, `UserName`, `Email`, `PhoneNumber`, `PasswordHash` gibi alanlar Identity'den hazır gelir.
- `<Guid>` ile Id tipi varsayılan `string` yerine `Guid` yapılır.
- Saf Clean Architecture'da Domain hiçbir pakete bağımlı olmamalıdır. Burada pragmatik bir tercih yapıldı ve Domain yalnızca `Microsoft.Extensions.Identity.Stores` paketine bağımlı.

### Tarihler neden `DateTime.UtcNow`?

PostgreSQL `timestamp with time zone` sütunlarında UTC olmayan `DateTime` değerlerini reddeder. Ayrıca sunucu ile istemcinin saat dilimi farklı olabilir. Tarihler UTC saklanır, gösterirken yerel saate çevrilir.

### Value object'ler hakkında not

Saf DDD'de value object'ler **değiştirilemez (immutable)** olur ve **değere göre eşitlik** kullanır. Buradaki sınıflar EF Core ile kolay çalışmak için `get; set;` ile basitleştirildi. Güncellemede eski nesne düzenlenmez, yenisiyle değiştirilir (`user.Address = request.Address?.ToEntity()`). Davranış olarak değiştirilemezlik mantığına uyulur.

/*
 * NEDEN VALUE OBJECT?
 *
 * 1. Değerleri aynı olan adresler → Value object yeterli.
 *    Ali ile Ayşe aynı apartmanda oturuyor. İkisinin de adresi "Kulas Light, Apt. 556, Gwenborough".
 *    Her kullanıcı kendi tablosundaki sütunlarda kendi kopyasını taşır.
 *    Ali taşınınca sadece Ali'nin adresi değişir, Ayşe'ninki etkilenmez. Bu doğru davranıştır.
 *    Burada entity'ye gerek yok, değerlerin tesadüfen aynı olması sorun değil.
 *
 * 2. Aynı adres kaydını paylaşmak → Entity gerekir.
 *    Bir şirketin 200 çalışanı aynı şube binasında. Şube taşındığında 200 kaydı tek tek güncellemek
 *    yerine tek bir adres kaydını güncelleyip hepsinin yeni adresi görmesi isteniyorsa,
 *    adresin kendi kimliği (Id) olur ve çalışanlar ona foreign key ile bağlanır.
 *    Adres artık "bir değer" değil, "bir şey" olmuştur. Bu da entity tanımıdır.
 *
 * Ayırt edici soru: "Bu adres değişince başka kayıtlar da otomatik değişmeli mi?"
 *    Hayır → value object  |  Evet → entity
 *
 * Bu projede her kullanıcının adresi kendine ait, biri değişince diğerleri etkilenmez → value object.
 * EF Core'da OwnsOne ile AspNetUsers tablosunda sütun olarak saklanır (bkz. AppDbContext).
 */