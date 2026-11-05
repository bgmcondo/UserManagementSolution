using UserManagement.Application.DTOs;

namespace UserManagement.Application.Interfaces
{
    //Kimlik doğrulama(giriş) işlemlerin sözleşmesi
    // Implementasyonu Application/Services/AuthService.cs.
    // AuthController bu arayüzü kullanır
    //
    //IUserService'ten neden ayrı?
    //Giriş yapmak kullanıcı yönetimi değil,kimlik doğrulama işi.Farklı sorumluluklar
    //farklı sınıflarda olmalı(Single Responsibality) . 
    //Ayrıca AuthController'ın UserService'in tüm metotlarına erişmesine gerek yok


    public interface IAuthService
    {
        //kullanıcı adı veya eposta ile giriş yapar
        //"@" içeriyorsa  e-postaya, içermiyorsa kullanıcı adına göre aranır
        //Parola doğruysa rolleriyle birlikte JWT token üretilip döner
        //
        // Kullanıcı bulunamazsa VEYA parola yanlışsa null döner, controller 401'e çevirir.
        // Hangisinin yanlış olduğu bilerek söylenmez: söylenirse saldırgan hangi
        // kullanıcı adlarının sistemde kayıtlı olduğunu öğrenebilir.
        //
        // Girdi geçersizse (boş alan vb.) ValidationException fırlatır, 400'e çevrilir.
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct);

    }
}
