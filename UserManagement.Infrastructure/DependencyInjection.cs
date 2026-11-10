using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Caching;
using UserManagement.Infrastructure.Security;
using UserManagement.Infrastructure.Seeding;
using UserManagement.Application.Abstractions;
using UserManagement.Application.Validators;   // ⚠ PasswordRules'un namespace'i neyse



namespace UserManagement.Infrastructure
{
    // Infrastructure katmanının DI kayıtları burada
    public static class DependencyInjection
    {
        // configuration: Program.cs'teki builder.Configuration'dan gelir.
        // WebApplication.CreateBuilder(args) çalışınca .NET ayarları otomatik olarak
        // şu kaynaklardan okuyup tek bir nesnede birleştirir:
        //   1. appsettings.json
        //   2. appsettings.Development.json  (Development ortamındaysa)
        //   3. User Secrets                   (Development ortamındaysa)
        //   4. Environment variable'lar
        //   5. Komut satırı argümanları
        // Aynı ayar birden fazla yerde varsa listede alttaki kazanır.
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //veritabanı bağlantısı
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            //identity 
            //UserManager<AppUser> ve RoleManager<IdentityRole<Guid>> bu kayıt sayesinde kullanılabilir  
            services.AddIdentityCore<AppUser>(options=>
            {
                // Şifre kuralları (appsettings'teki şifreler bunlara uymalı)
                options.Password.RequiredLength =PasswordRules.
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Aynı e-postayla iki kullanıcı olmasın
                options.User.RequireUniqueEmail = true;


            }
                
                
                )
        }





    }
}