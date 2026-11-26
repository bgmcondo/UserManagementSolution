
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagement.Domain.Entities;
using UserManagement.Domain.ValueObjects;
using UserManagement.Infrastructure.Seeding.Models;

namespace UserManagement.Infrastructure.Seeding
{
//uygulama açılırken veritabanına başlangıç kullanıcılarını ekler

     public sealed class DataSeeder
    {
        //Identity'nin kullanıcı yöneticisi:Kullanıcı oluşturur,şifreyi hashleyerek kaydeder.
        private readonly UserManager<AppUser> _userManager;

        // Rol adları; yazım hatası riskini önlemek için sabit
        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        //Dış adrese (jsonplaceholder) istek atmak için
        private readonly HttpClient _httpClient;

        //appsettings'teki seed ayarları
        private readonly SeedOptions _options;

        //konsola bilgi hata mesajı yazmak için
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(UserManager<AppUser>userManager,RoleManager<IdentityRole<Guid>> roleManager,HttpClient httpClient,IOptions<SeedOptions>options,ILogger<DataSeeder>logger)

        {
            _userManager = userManager;
            _roleManager=roleManager;
            _httpClient= httpClient;
            _options = options.Value;
            _logger = logger;
            
        }

        // Seeding işlemini başlatan ana metot
        public async Task SeedAsync(CancellationToken cancellationToken=default)
        {
        //1)Seeding kapalıysa hiçbir şey yapma
        if(!_options.Enabled)
            {
                _logger.LogInformation("Seeding kapalı,seeding atlanıyor");
                return;
            }
            await SeedRolesAsync();
            await SeedAdminAsync();
            // Roller ve adminler her ortamda gerekli; örnek kullanıcılar sadece geliştirmede
            if (_options.SampleUsersEnabled)
                await SeedExternalUserAsync(cancellationToken);
            else
                _logger.LogInformation("Örnek kullanıcılar kapalı (Seed:SampleUsersEnabled=false), atlanıyor.");

        }
        // Kullanıcıyı role ekler; önceki açılışta rolsüz kalmış kullanıcıyı da düzeltir.
        // Sadece uygulama açılışında çalışır, isteklerde değil.
        private async Task EnsureRoleAsync(AppUser user, string role)
        {
            // Ayrıca IsInRoleAsync çağırmıyoruz: AddToRoleAsync içeride zaten "rolde mi?" diye bakıyor.
            // [DB] Zaten roldeyse: ~2 SELECT (rolü bul + eşleşme var mı), hiçbir şey yazılmaz → "UserAlreadyInRole"
            // [DB] Rolsüzse: yukarıdakiler + ad/e-posta benzersizlik kontrolü (2 SELECT) + INSERT (rol eşleşmesi)
            var result = await _userManager.AddToRoleAsync(user, role);

            // [Bellek] "Zaten rolde" hata değil, istediğimiz durum; sadece gerçek hataları logla
            if (!result.Succeeded && result.Errors.Any(e => e.Code != "UserAlreadyInRole"))
                LogErrors(user.UserName ?? user.Email ?? "?", result);
        }

        // jsonplaceholder'dan kullanıcıları çekip ekler (her biri için: yoksa)
        private async Task SeedExternalUserAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.DefaultPassword))
            {
                _logger.LogWarning("Seed:DefaultPassword boş, örnek kullanıcılar eklenmiyor.");
                return;
            }
            //kullanıcı dışı adresten çek
            //GetFromJsonAsync:İsteği atar, gelen json'ı otomatik olarak listeye çevirir
            List<JsonPlaceholderUser>? externalUsers;
            try
            {
                externalUsers = await _httpClient.GetFromJsonAsync<List<JsonPlaceholderUser>>(_options.UsersUrl, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException      // site kapalı, internet yok
                                          or TaskCanceledException     // süre doldu
                                          or JsonException             // gelen veri bozuk
                                          or NotSupportedException)    // gelen cevap JSON değil
            {
                // Örnek kullanıcılar zorunlu değil: uyar ve devam et, API yine açılsın
                _logger.LogWarning(ex, "Örnek kullanıcılar alınamadı ({Url}), atlanıyor.", _options.UsersUrl);
                return;
            }
            if (externalUsers is null||externalUsers.Count==0)
            {
                _logger.LogWarning("Dış servisten kullanıcı gelmedi");
                return;

            }
            foreach (var externalUser in externalUsers)
            {
                //Eğer veritabanında bu emaile sahip şahıs varsa döngünün bu adımını atlıyoruz(continue) ile tekrar döngü başına geliyoruz
                //eğer tam tersi ise devam ediyoruz
                var existingUser = await _userManager.FindByEmailAsync(externalUser.Email);
                if (existingUser is not null)
                {
                    await EnsureRoleAsync(existingUser, UserRole);
                    continue;
                }
                var user = new AppUser
                {
                    UserName = externalUser.UserName,
                    Email = externalUser.Email,
                    EmailConfirmed=true,
                    PhoneNumber=externalUser.Phone,
                    FullName=externalUser.Name,
                    Website=externalUser.Website,
                    Address = MapAddress(externalUser.Address),
                    Company=MapCompany(externalUser.Company),
                
                };

                //Kullanıcıyı şifresiyle kaydediyoruz
                var result=await _userManager.CreateAsync(user,_options.DefaultPassword);
                //başarısızsa hatayı logluyoruz sıradaki kullanıcıya geçiyoruz
                if (!result.Succeeded)
                {
                    LogErrors(user.UserName, result);
                    continue;
                }
                //Normal kullanıcılara User rolü ver
                await EnsureRoleAsync(user, UserRole);
                _logger.LogInformation("Kullanıcı eklendi: {UserName}", user.UserName);

            }

         
                
         }

        private async Task SeedAdminAsync()
        {
            if (_options.Admins.Count == 0)
            {
                _logger.LogWarning("Seed:Admins boş, hiç admin oluşturulmuyor. Kullanıcı yönetimi yapılamaz.");
                return;
            }
            foreach (var a in _options.Admins)
            {
                //Bu epostayla kullanıcı zaten varsa tekrardan oluşturma, sadece rolünü kontrol et
                //FindByEmailAsync: Kullanıcıyı bulursa döndürür, bulamazsa null döner.
                var existingAdmin = await _userManager.FindByEmailAsync(a.AdminEmail);
                if (existingAdmin is not null)
                {
                    await EnsureRoleAsync(existingAdmin, AdminRole);
                    continue;
                }
                //Admin nesnesini hazırla(bilgiler appsettingsten geliyor)
                var admin = new AppUser
                {
                    UserName = a.AdminUserName,
                    Email = a.AdminEmail,
                    EmailConfirmed = true,// eposta doğrulanmış sayılsın,login engellenmesin
                    FullName =a.FullName

                };


                // kullanıcı kaydedilsin
                //createasync şifreyi hashleyip veritabanına yazar,sonucu döndürür
                var result = await _userManager.CreateAsync(admin,a.AdminPassword);


                //Kayıt başarısızsa(örn şifre kurallara uymuyorsa) hatayı logla çık
                if (!result.Succeeded)
                {
                    LogErrors(admin.UserName, result);
                    continue;
                }
                //Admin rolünü ver
                //JWT üretilirken u kullanıcının token'ına "role:Admin" claim'i buradan gelecek
                await EnsureRoleAsync(admin, AdminRole);
                _logger.LogInformation("Admin oluşturuldu: {Email}", admin.Email);
            }
        }
        //Identity  hatalarını tek satırda loglar
        private void LogErrors(string userName, IdentityResult result)
        {
            var errors = string.Join(",",result.Errors.Select(e=>e.Description));
            _logger.LogError("kullanıcı oluşturulamadı: {UserName}. Hatalar:{Errors}",userName,errors);
        }

        private async Task SeedRolesAsync()
        {
            foreach (var roleName in new[] { AdminRole, UserRole })
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                    continue;

           var result= await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                // Başarısızsa hatayı logluyoruz
                if (!result.Succeeded)
                {
                    _logger.LogError("Rol oluşturulamadı: {Role}. Hatalar: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }
                _logger.LogInformation("Rol oluşturuldu:{Role}",roleName);

                    }

        }

        private static Address? MapAddress(JsonPlaceholderAddress? source)
        {
            //json da adres yoksa boş bırak
            if (source is null) return null;

            return new Address
            {
                Street=source.Street,
                Suite=source.Suite,
                City=source.City,
                ZipCode=source.Zipcode,
                Geo=MapGeo(source.Geo)
            };
        }

        private static Company? MapCompany(JsonPlaceholderCompany? source)
        {
            if (source is null) return null;

            return new Company
            {
                Name = source.Name,
                CatchPhrase = source.CatchPhrase,
                Bs = source.Bs,
            };

        }


        // jsonplaceholder Lat/Lng'yi metin olarak gönderiyor ("-37.3159"); burada double'a çeviriyoruz
        private static GeoLocation? MapGeo(JsonPlaceholderGeo? source) { 

            //Geo bilgisi gelmediyse çevrilecek hiçbir şey yok
            if (source is null) return null;

            //lat ve lng'yi sayıya çevirmeye çalışıyoruz sonuçlar başarılıyla değerleri lat ve lng'ye yazıyoruz
            // InvariantCulture: Türkçe bilgisayarda da "." ondalık ayırıcı sayılsın
            if (!double.TryParse(source.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(source.Lng, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                return null;   // biri bile çevrilemezse Geo boş kalır, kullanıcı yine eklenir

            // İkisi de sayıya çevrildi: GeoLocation nesnesini oluştur
            return new GeoLocation { Latitude = lat, Longitude = lng };
        }


    }


}
