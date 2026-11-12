using UserManagement.Application.DTOs;   // ← bunu ekleyin

namespace UserManagement.Application.Interfaces
{
    public  interface IJwtTokenService
    {
        //kullanıcı giriş yaptığında çağrılır
        //Kullanıcının ID'si epostası ve rolleriyle imzalı bir jwt üretir
        //bu token her API isteğinde Authorization:Bearer <token> başlığıyla gönder
        //kısa ömürlüdür
        // IEnumerable<string> = “içinde string olan, foreach ile dolaşılabilen herhangi bir şey.”
        //Bu yüzden metoda liste de, dizi de verebilirsin, hepsi çalışır
       AccessTokenResult  GenerateAccessToken(Guid userId,string email, IEnumerable<string> roles);
        
        //token yenileme için kullanılacak rastgele bir değer üretir
        string GenerateRefreshToken();



    }
}
