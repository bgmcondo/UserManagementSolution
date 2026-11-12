using FluentValidation;
using Microsoft.Extensions.Options;
using UserManagement.Application.DTOs;
using UserManagement.Application.Options;

namespace UserManagement.Application.Validators;

public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator(IOptions<PasswordPolicyOptions> policyOptions)
    {
        var policy = policyOptions.Value;

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut parola boş olamaz.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .ValidPassword(policy)              // boşluk + uzunluk + harf/rakam kuralları
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("Yeni parola mevcut paroladan farklı olmalıdır.");
    }
}