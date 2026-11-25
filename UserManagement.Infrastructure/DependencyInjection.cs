using MassTransit.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Options;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Caching;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Security;
using UserManagement.Infrastructure.Seeding;

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

                // Brute-force koruması: 5 yanlış denemede hesap 15 dk kilitlenir
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
                .AddRoles<IdentityRole<Guid>>()               // RoleManager + AddToRoleAsync için
                .AddEntityFrameworkStores<AppDbContext>();    // kullanıcı ve rolleri bu DbContext'e kaydet

            // ---------- Repository ----------
            services.AddScoped<IUserRepository, UserRepository>();

            // ---------- Güvenlik ----------
            //jwt bölümünü jwt options ile bağlıyoruz
            //Jwt token service IOptions ile JwtOptions okuyor
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // ---------- Cache (Redis) ----------
            //Redis Connection'ı ekliyoruz
            var redisConnection = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis bağlantısı bulunamadı");
            // RedisCacheService'in RemoveByPrefixAsync / ExistsAsync için doğrudan Redis bağlantısı.
            // Singleton: bağlantı pahalıdır, uygulama boyunca tek bir tane kullanılır.
            services.AddSingleton<IConnectionMultiplexer>(_ => {

                var options = ConfigurationOptions.Parse(redisConnection);

                options.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(options);
            }
                );

            // IDistributedCache (Get/Set/Remove için)
           // yani bellekteki bilgi güncelleme ekleme silme vb şeyleri sağlayan hizmetin kodunun kaydı
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = RedisCacheService.InstanceName;



            }
                );


            //Seed bölümünü seed options'a bağlıyoruz
            services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

            // DataSeeder dış API'ye (jsonplaceholder) istek attığı için HttpClient ile kaydediliyor
            services.AddHttpClient<DataSeeder>();




            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}