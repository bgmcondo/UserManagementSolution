using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators
{
    // Yeni kullanıcı ekleme isteğinin doğrulaması (POST /api/users).
    // Kurallardan biri bozulursa API 400 Bad Request döner.
    //
    // Parola kuralları, Identity'nin parola ayarlarıyla AYNI olmalı.
    // Farklı olursa validator kabul eder ama Identity reddeder ve kullanıcı
    // kafa karıştırıcı bir hata görür.
    public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
    {
      
            public CreateUserRequestDtoValidator() {
            //Kullanıcı adında "@" olamaz. Böylece kullanıcı adı ile e-posta hiçbir zaman karışmaz.
            // Kullanıcı adı kuralları UserNameRules'ta tanımlı (ChangeUserNameRequestDtoValidator ile ortak).
            RuleFor(x => x.UserName).ValidUserName();

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi girilmelidir.");

            RuleFor(x => x.Password).ValidPassword();
             

            RuleFor(x => x.PhoneNumber).MaximumLength(30).WithMessage("Telefon numarası en fazla 30 karakter olabilir.");
            RuleFor(x => x.Website).MaximumLength(200).WithMessage("Web sitesi en fazla 200 karakter olabilir.");

            //İç içe nesneler:gönderildiyse kendi validator'larıyla doğrulanır. Gönderilmediyse (null) hata vermez.
            //! bu işaret null olamayacağını söylüyor.
            RuleFor(x => x.Address!).SetValidator(new AddressDtoValidator()).When(x => x.Address != null);

            RuleFor(x => x.Company!).SetValidator(new CompanyDtoValidator()).When(x => x.Company != null);



        }
    }
}
