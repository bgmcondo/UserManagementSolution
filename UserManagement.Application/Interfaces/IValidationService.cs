namespace UserManagement.Application.Interfaces
{
    // Doğrulama sözleşmesi. Servisler FluentValidation'ı bilmez, sadece bunu bilir.
    public interface IValidationService
    {
        // Geçersizse bizim ValidationException'ımızı fırlatır
        Task ValidateAsync<T>(T instance, CancellationToken ct = default);
    }
}