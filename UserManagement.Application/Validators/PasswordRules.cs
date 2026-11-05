using FluentValidation;
namespace UserManagement.Application.Validators
{
    public static class PasswordRules
    {
        public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]+").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]+").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]+").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[\!\?\*\.]+").WithMessage("Şifre en az bir özel karakter (!? *.) içermelidir.");
        }
    }
}
