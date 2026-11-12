using MassTransit.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Security
{
    public  class JwtTokenService : IJwtTokenService
    {
        //appsettingjsondan gelen jwt ayarları
        private readonly JwtOptions _options;

        //appsettingjson içinden bu gerekli değerleri Options sistemi okuyor
        //IOptions paketine koyuyor ve servise iletiyor

        public JwtTokenService(IOptions<JwtOptions>options)
        {
            _options = options.Value;
            
        }
        public AccessTokenResult GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
        {
            //claimler token içine yazılması gereken bilgiler
            var claims = new List<Claim>
            {
                //sub email service de konu olarak kullanılsa da token service de tokenın kim için üretildiğini gösteriyor  
                new(JwtRegisteredClaimNames.Sub,userId.ToString()),
                new(JwtRegisteredClaimNames.Email,email),
                new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) //tokena özel benzersiz id
            };

            foreach (var role in roles)
            {

                claims.Add(new Claim(ClaimTypes.Role,role)); //HER ROL AYRI CLAİM
            }
            //Gizli anahtardan imzalama anahtarı oluşturulur
            var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            // imza:(anahtar,algoritma)
            var credentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            //token tarifi 
            var now=DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_options.AccessTokenExpirationMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                //token'ın taşıdığı bilgiler
                Subject=new ClaimsIdentity(claims),

                //tokenı kim üretti
                Issuer=_options.Issuer,

                //Token hangi uygulama için=> istemci uygulama
                Audience=_options.Audience,

                //Token ne zaman üretildi
                IssuedAt=now,

                //token bu andan önce geçersiz(hemen geçerli olsun diye now)
                NotBefore=now,

                //Token ne zaman geçersiz olacak(örn 60dk sonra)
                Expires=expiresAt,

                // Token hangi anahtar ve algoritmayla imzalanacak (3. adımda hazırlamıştık)
                SigningCredentials = credentials

            };

            //token'ı tarife göre oluştur ve string olarak döndür
            var handler= new JsonWebTokenHandler();
            var token=handler.CreateToken(descriptor);
            return new AccessTokenResult(token, expiresAt);
        }
        //Access token yenilemede kullanılacak refresh token'ı üretir
        public string GenerateRefreshToken()
        {
            //kriptografik olarak güvenli 64 byte rastgele veri
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            //metne çevir(veri tabanında saklamak ve istemciye göndermek için)
            return Convert.ToBase64String(randomBytes); 
        }
    }
}
