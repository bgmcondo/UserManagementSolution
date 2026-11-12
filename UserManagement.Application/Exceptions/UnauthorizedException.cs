namespace UserManagement.Application.Exceptions
{
    // Kimlik doğrulanamadıysa fırlatılır → 401 Unauthorized.
    // Örnek: login'de yanlış kullanıcı adı/parola, geçersiz veya süresi dolmuş refresh token.
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Kimlik doğrulanamadı.") : base(message) { }
    }
}