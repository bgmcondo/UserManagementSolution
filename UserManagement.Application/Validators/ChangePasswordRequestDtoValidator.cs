using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators;

// Parola değiştirme isteğinin doğrulaması (PATCH /api/users/me/password).
//
// CurrentPassword: sadece boş olmadığı kontrol edilir, doğruluğuna Identity karar verir.
// NewPassword: Create ile aynı kurallar (PasswordRules.ValidPassword).
//              Ayrıca mevcut paroladan farklı olmalı.
public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut parola boş olamaz.");

        RuleFor(x => x.NewPassword)
            .ValidPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni parola mevcut paroladan farklı olmalıdır.");
    }
}