using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using ValidationException = UserManagement.Application.Exceptions.ValidationException;

namespace UserManagement.Application.Services
{
    // FluentValidation SADECE burada geçer. Tipe göre doğru validator'ı DI'dan bulur.
    public class ValidationService : IValidationService
    {
        // Hangi validator'ın gerekeceği ancak ValidateAsync<T> çağrıldığında (T belli olunca) anlaşılır.
        // Constructor'da önceden isteyemediğimiz için DI deposunun kendisini alıp, ihtiyaç anında içinden çekiyoruz.
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }
        public async Task ValidateAsync<T>(T instance, CancellationToken ct = default)
        {
            // T = UpdateUserRequestDto ise → DI deposundan UpdateUserRequestDtoValidator gelir
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator is null)
                return;   // Bu tip için validator yazılmamış, doğrulanacak kural yok
                          // Kuralları çalıştır (hata fırlatmaz, sadece sonucu döner)
            var result = await validator.ValidateAsync(instance, ct);
            if (result.IsValid)
                return;

            // hataları alan adına göre grupla: "Email" → ["...", "..."]
            var errors = result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            // Middleware bunu yakalar → 400 + errors listesi döner
            throw new ValidationException(errors);


        }
    }
}
