using FluentValidation;
// FluentValidation'da da "ValidationException" var. Bu dosyada BİZİMKİ anlaşılsın diye takma ad veriyoruz.
using ValidationException = UserManagement.Application.Exceptions.ValidationException;

namespace UserManagement.Application.Common
{
    //fluentvalidation için yardımcı metotlar
    //amacımız fluentvalidationun exceptionu yerine kendi exceptionumuzu fırlatmak
    public static class ValidatorExtensions
    {
        // Kullanım: await _createValidator.ValidateOrThrowAsync(request, ct);
        // "this" → extension metot: validator'ın kendi metoduymuş gibi çağrılır.
        public static async Task ValidateOrThrowAsync<T>(
            this IValidator<T> validator,
            T instance,
            CancellationToken ct = default)
        {

            //Kuralları çalıştırır(hata fırlatmaz,sadece sonucu döner
            var result=await validator.ValidateAsync(instance, ct);

            //herşey geçerliyse çıkar,metot normal devam  eder
            if (result.IsValid)
                return;

            // Hataları alan adına göre grupla:
            // "Email" → ["E-posta boş olamaz.", "Geçerli bir e-posta adresi girilmelidir."]
            //g parametresi grubu temsil ediyor
            
            var errors= result.Errors.GroupBy(e=>e.PropertyName).ToDictionary(g=>g.Key, g=>g.Select(e=>e.ErrorMessage).ToArray());

            // Bizim exception'ımız → middleware 400 + errors listesi döner
            throw new ValidationException(errors);


        }


    }
}
