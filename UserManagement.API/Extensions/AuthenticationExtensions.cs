using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Data.Entity.Core.Mapping;
using System.Text;
using UserManagement.Infrastructure.Security;

namespace UserManagement.API.Extensions
{
    //jwt doğrulama ayarlarını program.cs ten ayırmak için
    // Token'ı ÜRETEN: Infrastructure/Security/JwtTokenService
    // Token'ı DOĞRULAYAN: burası (her istekte UseAuthentication middleware'i çalıştırır)

    //   Bearer       → kural: token'ı olan girer
    //   AddJwtBearer → kapıdaki görevli: token sahte mi, süresi dolmuş mu diye bakar
    //   JwtBearerDefaults.AuthenticationScheme → o görevlinin adı ("Bearer")
    public static class AuthenticationExtensions
    {
        public static   IServiceCollection  AddJWTAuthentication(this IServiceCollection services,IConfiguration configuration)
        {
            // appsettings.json'daki "jwt" bölümünü oku
            var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("appsettings.json içinde 'jwt' bölümü bulunamadı.");

            services
                //kimlik doğrulamayı açıyoruz ve varsayılan görevliyi seçiyoruz
                // JwtBearerDefaults.AuthenticationScheme = "Bearer" → görevlinin adı.
                // [Authorize] gördüğü her yerde, başka bir şey belirtilmediyse bu görevli çalışır.
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            // "Bearer" adlı görevliyi işe alır: gelen token sahte mi, süresi dolmuş mu,
            // bizim için mi üretilmiş diye kontrol eder (ayarları aşağıda).

              .AddJwtBearer(options =>
             {
                 //KONTROL LİSTESİ
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true, //Kontrol yapılsın mı?
                     ValidIssuer = jwt.Issuer, //Kontrol neye göre yapılsın

                     //token kimin için üretildi
                     ValidateAudience = true,
                     ValidAudience = jwt.Audience,

                     //token süresi dolmuş mu
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.Zero, //varsayılan 5 dk tolerans var,sıfırlıyoruz

                     //imza gerçek mi 

                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SecretKey))


                 };

             });






            return services;                                                              

        }
    }
}
