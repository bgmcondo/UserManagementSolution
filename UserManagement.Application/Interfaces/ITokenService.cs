using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces
{
    //JWT token üretiminin sözleşmesi
    // Implementasyonu Infrastructure/Security/JwtTokenService.cs.
    // AuthService giriş başarılı olunca bu arayüzü kullanır.
    //
    // Neden arayüz?
    //   Token'ın içeriği (claim'ler: Id, kullanıcı adı, e-posta, roller) kullanıcıdan gelir
    //   ve parametre olarak verilir. Ama token'ı imzalamak için gizli anahtar (Jwt:Key),
    //   süre ve imzalama algoritması da gerekiyor. Bunlar dış yapılandırmaya ve kütüphaneye
    //   (System.IdentityModel.Tokens.Jwt) bağlı. Application bu ayrıntıları bilmemeli.
    //   Ayrıca AuthService testlerinde gerçek token üretmek yerine sahte bir
    //   LoginResponseDto döndüren mock kullanılabilir.
    //
    //Neden async değil?
    //Token üretmek bellekte yapılan bir hesaplama:veritabanına ağa veya diske gitmez
    //I/O olmadığı için task ve cancellationa gerek yok

    public interface ITokenService
    {
        //Kullanıcı bilgileri ve rolleriyle imzalı bir JWT üretir
        // Token'a yazılan bilgiler claim:
        //kullanıcı adı,eposta
        //Roller [Authorize(Roles="Admin)] bu claim'e bakar
        //
        //Roller ayrı parametre olarak geliyor, çünkü AppUser'ın içinde bulunmuyor
        //AuthService önce repository'den rolleri alıp buraya verir.
        LoginResponseDto CreateToken(AppUser user, IList<string> roles);
    }
}
