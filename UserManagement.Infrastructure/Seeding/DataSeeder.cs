
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
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
            await SeedExternalUserAsync(cancellationToken);

        }
        // jsonplaceholder'dan kullanıcıları çekip ekler (her biri için: yoksa)
        private async Task SeedExternalUserAsync(CancellationToken cancellationToken)
        {
            //kullanıcı dışı adresten çek
            //GetFromJsonAsync:İsteği atar, gelen json'ı otomatik olarak listeye çevirir
            var externalUsers = await _httpClient.GetFromJsonAsync<List<JsonPlaceholderUser>>(_options.UsersUrl, cancellationToken);
            if(externalUsers is null||externalUsers.Count==0)
            {
                _logger.LogWarning("Dış servisten kullanıcı gelmedi");
                return;

            }
            foreach (var externalUser in externalUsers)
            {
                if (await _userManager.FindByEmailAsync(externalUser.Email) is not null)
                    continue;

                //Eğer veritabanında bu emaile sahip şahıs varsa döngünün bu adımını atlıyoruz(continue) ile tekrar döngü başına geliyoruz
                //eğer tam tersi ise devam ediyoruz
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
                    //Normal kullanıcılara User rolü ver
                }
                await _userManager.AddToRoleAsync(user, UserRole);
                _logger.LogInformation("Kullanıcı eklendi: {UserName}", user.UserName);

            }

         
                
         }

        private async Task SeedAdminAsync()
        {
            //Bu epostayla kullanıcı zaten varsa tekrardan oluşturma 
            //FindByEmailAsync: Kullanıcıyı bulursa döndürür, bulamazsa null döner.
            if (await _userManager.FindByEmailAsync(_options.AdminEmail) is not null)
                return;
            //Admin nesnesini hazırla(bilgiler appsettingsten geliyor)
            var admin = new AppUser
            {
                UserName=_options.AdminUserName,
                Email=_options.AdminEmail,
                EmailConfirmed=true,// eposta doğrulanmış sayılsın,login engellenmesin
                FullName="System Administor"

            };
            // kullanıcı kaydedilsin
            //createasync şifreyi hashleyip veritabanına yazar,sonucu döndürür
            var result=await _userManager.CreateAsync(admin,_options.AdminPassword);

            //Kayıt başarısızsa(örn şifre kurallara uymuyorsa) hatayı logla çık
            if (!result.Succeeded)
            {
                LogErrors(admin.UserName, result);
                return;
            }
            //Admin rolünü ver
            //JWT üretilirken u kullanıcının token'ına "role:Admin" claim'i buradan gelecek
            await _userManager.AddToRoleAsync(admin, AdminRole);
            _logger.LogInformation("Admin oluşturuldu: {Email}", admin.Email);
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

            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                _logger.LogInformation("Rol oluşturuldu:{Role}",roleName);

                    }

        }

        private static Address? MapAddress(JsonPlaceholderAddress source)
        {
            //json da adres yoksa boş bırak
            if (source is null) return null;

            return new Address
            {
                Street=source.Street,
                Suite=source.Suite,
                City=source.City,
                ZipCode=source.Zipcode
            };
        }

        private static Company? MapCompany(JsonPlaceholderCompany source)
        {
            if (source is null) return null;

            return new Company
            {
                Name = source.Name,
                CatchPhrase = source.CatchPhrase,
                Bs = source.Bs,
            };

        }

    




        }


    }
