using FluentValidation;
using UserManagement.Application.Options;

namespace UserManagement.Application.Validators;

public static class PasswordRules
{
    // Not: Kurallar ASP.NET Identity'nin PasswordValidator'ı ile aynı (ASCII).
    // ç, ş, ğ gibi harfler Identity'de de özel karakter sayılır.
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        PasswordPolicyOptions policy)
    {
        // Her zaman geçerli kurallar: boş olmasın, en az MinLength karakter olsun
        var rule = ruleBuilder
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(policy.MinLength).WithMessage($"Şifre en az {policy.MinLength} karakter olmalıdır.");

        // Ayarda açık olan kuralları ekle
        if (policy.RequireUppercase)
            rule = rule.Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.");

        if (policy.RequireLowercase)
            rule = rule.Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.");

        if (policy.RequireDigit)
            rule = rule.Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        if (policy.RequireNonAlphanumeric)
            rule = rule.Matches("[^a-zA-Z0-9]").WithMessage("Şifre en az bir özel karakter içermelidir.");

        return rule;
    }
}