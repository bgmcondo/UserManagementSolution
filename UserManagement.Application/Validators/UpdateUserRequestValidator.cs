using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators;

// Kullanıcı profil güncelleme isteğinin doğrulaması.
// İki endpoint'te ortak kullanılır: PUT /api/users/{id} (Admin) ve PUT /api/users/me (kendisi).
// İki endpoint de aynı DTO'yu aldığı için kurallar da aynı.
//
// CreateUserRequestDtoValidator'dan farkı:
//   - UserName kuralı yok → kullanıcı adı ChangeUserNameRequestDtoValidator'da
//   - Password kuralı yok → parola bu istekte bulunmuyor
//
// Diğer kurallar Create ile AYNI olmalı. Aksi halde eklenirken kabul edilen bir değer
// (örneğin 120 karakterlik ad) güncellenirken reddedilir ya da tam tersi olur.
public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girilmelidir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad boş olamaz.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.PhoneNumber).MaximumLength(30).WithMessage("Telefon numarası en fazla 30 karakter olabilir.");
        RuleFor(x => x.Website).MaximumLength(200).WithMessage("Web sitesi en fazla 200 karakter olabilir.");

        // İç içe nesneler: gönderildiyse kendi validator'larıyla doğrulanır. Gönderilmediyse (null) hata vermez.
        // "!" derleyiciye "When sayesinde burada null değil" diyor.
        RuleFor(x => x.Address!).SetValidator(new AddressDtoValidator()).When(x => x.Address != null);
        RuleFor(x => x.Company!).SetValidator(new CompanyDtoValidator()).When(x => x.Company != null);
    }
}