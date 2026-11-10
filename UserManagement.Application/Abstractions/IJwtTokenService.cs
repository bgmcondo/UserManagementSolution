using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace UserManagement.Application.Abstractions
{
    public  interface IJwtTokenService
    {
        //kullanıcı giriş yaptığında çağrılır
        //Kullanıcının ID'si epostası ve rolleriyle imzalı bir jwt üretir
        //bu token her API isteğinde Authorization:Bearer <token> başlığıyla gönder
        //kısa ömürlüdür
        // IEnumerable<string> = “içinde string olan, foreach ile dolaşılabilen herhangi bir şey.”
        //Bu yüzden metoda liste de, dizi de verebilirsin, hepsi çalışır
        string GenerateAccessToken(Guid userId,string email, IEnumerable<string> roles);
        
        //token yenileme için kullanılacak rastgele bir değer üretir
        string GenerateRefreshToken();



    }
}
