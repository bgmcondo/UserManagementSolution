namespace UserManagement.Application.Exceptions
{
    // İstemcinin gönderdiği veri geçersizse fırlatılır → 400 Bad Request.
    // Örnek: zayıf parola, yanlış mevcut parola, Identity'nin reddettiği girdi.
    // Errors: alan adı → hata mesajları (ör. "Password" → ["En az 8 karakter olmalı"]).
    public class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Bir veya daha fazla doğrulama hatası oluştu.")
        {
            Errors = new Dictionary<string, string[]>(errors);
        }
    }
}