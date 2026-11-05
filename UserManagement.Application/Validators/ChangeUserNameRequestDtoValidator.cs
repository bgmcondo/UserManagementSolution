using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators;

// Kullanıcının kendi kullanıcı adını değiştirme isteğinin doğrulaması
// (PATCH /api/users/me/username).
//
// NewUserName: Create ile aynı kurallar (UserNameRules).
// CurrentPassword: sadece boş olmadığı kontrol edilir. Doğru olup olmadığına Identity karar verir.
//   Güç kuralları (büyük harf, rakam vb.) burada uygulanmaz, çünkü yeni parola belirlemiyoruz,
//   mevcut parolayı doğruluyoruz.
//
// Yeni adın başka bir kullanıcıda olup olmadığını validator değil Identity kontrol eder,
// çünkü bunun için veritabanına sormak gerekiyor.
public class ChangeUserNameRequestDtoValidator : AbstractValidator<ChangeUserNameRequestDto>
{
    public ChangeUserNameRequestDtoValidator()
    {
        RuleFor(x => x.NewUserName).ValidUserName();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut parola boş olamaz.");
    }
}