namespace UserManagement.Application.Exceptions
{
    // Kimlik doğrulandı ama bu işleme yetkisi yoksa fırlatılır → 403 Forbidden.
    // Örnek: Admin'in kendi hesabını silmesi, kullanıcının başkasının profilini değiştirmesi.
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Bu işlem için yetkiniz yok.") : base(message) { }
    }
}