using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Options;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Caching;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Security;

namespace UserManagement.Infrastructure
{
    // Infrastructure katmanının DI kayıtları burada.
    // Program.cs'te builder.Services.AddInfrastructure(builder.Configuration) ile çağrılır.
    //
    // configuration: Program.cs'teki builder.Configuration'dan gelir.
    // .NET ayarları şu kaynaklardan okuyup birleştirir (aynı ayar birden fazla yerdeyse alttaki kazanır):
    //   1. appsettings.json
    //   2. appsettings.Development.json  (Development ortamındaysa)
    //   3. User Secrets                   (Development ortamındaysa)
    //   4. Environment variable'lar
    //   5. Komut satırı argümanları
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ---------- Ayarlar ----------
            // "PasswordPolicy" bölümünü oku. Bölüm yoksa sınıftaki varsayılanlar kullanılır.
            var passwordSection = configuration.GetSection(PasswordPolicyOptions.SectionName);
            var passwordPolicy = passwordSection.Get<PasswordPolicyOptions>() ?? new PasswordPolicyOptions();

            // Validator'lar IOptions<PasswordPolicyOptions> ile okuyabilsin diye kaydet
            services.Configure<PasswordPolicyOptions>(passwordSection);

            // ---------- Veritabanı ----------
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // ---------- Identity ----------
            // UserManager<AppUser> ve RoleManager<IdentityRole<Guid>> bu kayıt sayesinde kullanılabilir.
            services.AddIdentityCore<AppUser>(options =>
            {
                // Parola kuralları: validator'larla aynı kaynaktan (PasswordPolicyOptions)
                options.Password.RequiredLength = passwordPolicy.MinLength;
                options.Password.RequireDigit = passwordPolicy.RequireDigit;
                options.Password.RequireUppercase = passwordPolicy.RequireUppercase;
                options.Password.RequireLowercase = passwordPolicy.RequireLowercase;
                options.Password.RequireNonAlphanumeric = passwordPolicy.RequireNonAlphanumeric;

                // Aynı e-postayla iki kullanıcı olmasın
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole<Guid>>()               // RoleManager + AddToRoleAsync için
                .AddEntityFrameworkStores<AppDbContext>();    // kullanıcı ve rolleri bu DbContext'e kaydet

            // ---------- Repository ----------
            services.AddScoped<IUserRepository, UserRepository>();

            // ---------- Güvenlik ----------
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // ---------- Cache (Redis) ----------
            services.AddStackExchangeRedisCache(options =>
                options.Configuration = configuration.GetConnectionString("Redis"));
            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}