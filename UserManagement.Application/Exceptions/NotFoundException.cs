namespace UserManagement.Application.Exceptions
{
    // Aranan kayıt bulunamadığında fırlatılır. GlobalExceptionHandler bunu 404'e çevirir.
    // KeyNotFoundException'dan türetilmiyor: .NET de onu fırlatır (ör. Dictionary'de olmayan anahtar),
    // ikisi aynı hiyerarşide olursa gerçek bug'lar yanlışlıkla 404 olarak dönebilir.
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}