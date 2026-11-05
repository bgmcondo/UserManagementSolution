using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators
{
    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        //Adress doğrulaması
        //Tüm alanlar isteğe bağlıdır,ama uzunluk sınırı var.
        //Uzunluk sınırları, veritabanındaki sütun boyutları ile uyumlu olmalıdır.
        //Aksi halde validator kabul eder , ama veritabanına yazarken hata alırız.


        public AddressDtoValidator()
        {
            RuleFor(x => x.Street).MaximumLength(150);
            RuleFor(x => x.Suite).MaximumLength(100);
            RuleFor(x => x.City).MaximumLength(100);
            RuleFor(x => x.ZipCode).MaximumLength(20);
        }
    }
}
