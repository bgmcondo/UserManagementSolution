using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UserManagement.Application.Common;
using UserManagement.Application.Interfaces;
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


            // Hatalı ayarı ilk login'de 500 olarak değil, açılışta net bir mesajla yakala
            if (Encoding.UTF8.GetByteCount(jwt.SecretKey) < JwtOptions.MinSecretKeyBytes)
                throw new InvalidOperationException(
                    $"Jwt:SecretKey en az {JwtOptions.MinSecretKeyBytes} byte olmalı (şu an {Encoding.UTF8.GetByteCount(jwt.SecretKey)}). " +
                    "user-secrets veya ortam değişkeninden daha uzun bir anahtar verin.");

            if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException("Jwt:Issuer ve Jwt:Audience boş olamaz.");

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

                 options.Events = new JwtBearerEvents
                 {
                     OnMessageReceived = context=>
                     {

                         var accessToken = context.Request.Query["access_token"]; //token alınıyor

                         var path = context.HttpContext.Request.Path; //istek nereye gidiyor

                         // Hub isteğiyse URL'deki token'ı JwtBearer'a verir; o da header'dan gelmiş gibi doğrular (imza, süre vb.)
                         if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                             context.Token = accessToken;
                              

                         return Task.CompletedTask;
                     },

                     //İMZA VE SÜRE GEÇERLİ OLSA BİLE:TOKENDAKİ STAMP GÜNCEL DEĞİLSE REDDEDİYORUZ
                     // OnTokenValidated: imza ve süre kontrolü geçtikten sonra çalışır.
                     // Burada ek olarak stamp'e bakıyoruz; eşleşmiyorsa (parola/e-posta değişmiş
                     // veya kullanıcı silinmiş) token'ı reddediyoruz.
                     //OnTokenValidated:imza ve süre kontrolü bittikten sonra çalışıyor
                     OnTokenValidated = async context =>
                     {

                         var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                         var tokenStamp = context.Principal?.FindFirstValue(CustomClaimTypes.SecurityStamp);

                         if(!Guid.TryParse(userIdValue, out var userId )||string.IsNullOrEmpty(tokenStamp))
                         {
                             context.Fail("Token eksik bilgi içeriyor.");
                             return;
                         }

                         var stampService=context.HttpContext.RequestServices.GetRequiredService<ISecurityStampService>();

                         var currentStamp = await stampService.GetCurrentAsync(userId, context.HttpContext.RequestAborted);

                         
                         if(currentStamp is null||currentStamp != tokenStamp)
                         {
                             context.Fail("Token artık geçerli değil. Lütfen tekrar giriş yapın.");

                         }


                     }


                 };

             });






            return services;                                                              

        }
    }
}
