using FluentValidation;

namespace UserManagement.Application.Validators
{
    public class CompanyDtoValidator : AbstractValidator<CompanyDto>
    {
        // Şirket bilgisi doğrulaması.
        // Tüm alanlar isteğe bağlı, sadece uzunluk sınırı var.

        public CompanyDtoValidator()
        {
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.CatchPhrase).MaximumLength(250);
            RuleFor(x => x.Bs).MaximumLength(250);
        }
    }
}
