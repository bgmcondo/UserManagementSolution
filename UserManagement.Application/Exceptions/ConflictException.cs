namespace UserManagement.Application.Exceptions
{
    // Kayıt zaten varsa fırlatılır → 409 Conflict.
    // Örnek: aynı e-posta veya kullanıcı adıyla ikinci kayıt.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}