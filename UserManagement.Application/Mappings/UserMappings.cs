
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.ValueObjects;



namespace UserManagement.Application.Mappings
{
    // Bu sınıf, kullanıcı ile ilgili veri transfer nesneleri (DTO) ve domain varlıkları arasında dönüşümleri tanımlar. Bu sayede uygulama katmanı, domain katmanındaki karmaşık yapıları bilmek zorunda kalmadan veri alışverişi yapabilir.
    //
    //Neden static?
    //Extension metotlar (ilk parametre this olan metotlar) c# kuralı gereği
    // Ayrıca bu metotlar state tutmaz, yani sınıfın örneklenmesine gerek yoktur. Bu yüzden static olarak tanımlanır.
    //Veri tabanına bağlı değil, sadece veri transferi ile ilgilidir.Aynı girdi her zaman aynı çıktıyı üretir. Bu yüzden stateless ve static olarak tanımlanır.


    // Neden AutoMapper / Mapster / Mapperly değil?
    //Tek entity olduğunda, manuel mapping yapmak daha hızlı ve anlaşılırdır. Mapper kütüphaneleri, özellikle küçük projelerde, ek bağımlılık ve karmaşıklık getirir. Ayrıca, manuel mapping ile dönüşüm mantığını açıkça görebiliriz ve gerektiğinde özelleştirebiliriz.

    public static class UserMappings
    {

        // Domain entity'leri ile DTO'lar arasındaki dönüşümleri yapan yardımcı sınıf.
        //
        // Neden static?
        //   Extension method'lar (ilk parametresinde "this" olan metotlar) C# kuralı gereği
        //   yalnızca static sınıflarda tanımlanabilir. Ayrıca bu metotlar durum (state) tutmuyor
        //   ve dışarıya (veritabanı, Redis vb.) bağımlı değil. Aynı girdi her zaman aynı çıktıyı
        //   verir (saf fonksiyon). Bu yüzden sınıftan nesne oluşturmaya veya DI ile vermeye gerek yok.
        //
        // Neden içeride "new" var?
        //   Static olan, dönüşümü yapan sınıf. Dönüşümün SONUCU (DTO veya value object) ise
        //   her çağrıda yeni oluşturulması gereken bir veri nesnesi.
        //
        // Neden AutoMapper / Mapster / Mapperly değil?
        //   Tek entity olduğu için kütüphane bağımlılığı eklemek yerine elle yazıldı.
        //   Hangi alanın nereye gittiği açıkça görünür. Bir alan adı yanlış yazılırsa
        //   hata çalışma anında değil, derleme anında ortaya çıkar.
        //
        // Null kontrolü:
        //   Metotların içinde null kontrolü yapılmaz. Çağıran taraf "?." kullanır:
        //     u.Address?.ToDto()  → adres null ise metot hiç çağrılmaz, sonuç null olur.
        //   Böylece metoda giren nesne her zaman doludur.
        //
        // Kullanım:
        //   var response = user.ToResponseDto();            // AppUser → UserResponseDto
        //   user.Address = request.Address?.ToEntity();     // AddressDto → Address


        // AppUser → UserResponseDto

        public static UserResponseDto ToResponseDto(this AppUser user) => new(
            user.Id,
            user.UserName!,
            user.Email!,
            user.FullName,
            user.PhoneNumber,
            user.Website,
            user.Address?.ToDto(),
            user.Company?.ToDto(),
            user.CreatedAt,
            user.UpdatedAt
        );



        // Address → AddressDto
        public static AddressDto ToDto(this Address address) => new(
            address.Street,
            address.Suite,
            address.City,
            address.ZipCode,
            address.Geo?.ToDto()
        );

        //GeoLocation → GeoDto
        public static GeoDto ToDto(this GeoLocation geo) => new(
            geo.Latitude,
            geo.Longitude
        );

        // Company → CompanyDto

        public static CompanyDto ToDto(this Company company) => new(
            company.Name,
            company.CatchPhrase,
            company.Bs
        );


        // ---------- DTO → Entity (içeriye gelen) ----------
        // UserService'te kullanıcı oluşturulurken ve güncellenirken kullanılır.
        //
        // Not: AppUser için ToEntity metodu yok. Kullanıcıyı UserService içinde elle oluşturuyoruz,
        // çünkü CreatedAt atamak ve parolayı ayrı tutmak gibi iş kuralları var.
        // Bunlar mapping'in değil servisin sorumluluğu.
        //
        // Güncellemede eski adres/şirket düzenlenmez, bu metotlarla oluşturulan yeni nesneyle
        // değiştirilir. Böylece value object'lerin "değiştirilemezlik" mantığına uyulmuş olur.


        // İstemciden gelen GeoDto'yu GeoLocation value object'ine çevirir.
        public static GeoLocation ToEntity(this GeoDto d) => new()
        {
            Latitude = d.Latitude,
            Longitude = d.Longitude
        };
        // İstemciden gelen AddressDto'yu Address value object'ine çevirir.
        public static Address ToEntity(this AddressDto d) => new()
        {
            Street = d.Street,
            Suite = d.Suite,
            City = d.City,
            ZipCode = d.ZipCode,
            Geo = d.Geo?.ToEntity()   // koordinat yoksa null
        };

        // İstemciden gelen CompanyDto'yu Company value object'ine çevirir.

        public static Company ToEntity(this CompanyDto d) => new()
        {
            Name = d.Name,
            CatchPhrase = d.CatchPhrase,
            Bs = d.Bs
        };



    }
}
