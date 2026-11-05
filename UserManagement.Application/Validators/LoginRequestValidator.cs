using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators;

// Giriş isteğinin doğrulaması (POST /api/auth/login).
//
// UserNameOrEmail için EmailAddress kuralı YOK, çünkü gelen değer kullanıcı adı da olabilir.
// Parolanın sadece boş olmadığı kontrol edilir. Güç kuralları burada uygulanmaz:
// parolanın doğru olup olmadığına Identity karar verir.
public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty().WithMessage("Kullanıcı adı veya e-posta boş olamaz.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.");
    }
}