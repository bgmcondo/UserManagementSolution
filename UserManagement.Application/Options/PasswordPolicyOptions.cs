namespace UserManagement.Application.Options
{
    // Şifre politikası (Options Pattern).
    // Değerler appsettings.json'daki "PasswordPolicy" bölümünden okunur.
    // Hem FluentValidation (PasswordRules) hem Identity bu ayarları kullanır;
    // böylece kurallar tek yerde tanımlanır.
    public sealed class PasswordPolicyOptions
    {
        public const string SectionName = "PasswordPolicy";

        // En az şifre uzunluğu
        public int MinLength { get; init; } = 8;

        // En az bir rakam zorunlu mu?
        public bool RequireDigit { get; init; } = true;

        // En az bir büyük harf zorunlu mu?
        public bool RequireUppercase { get; init; } = true;

        // En az bir küçük harf zorunlu mu?
        public bool RequireLowercase { get; init; } = true;

        // En az bir özel karakter (harf ve rakam dışı) zorunlu mu?
        public bool RequireNonAlphanumeric { get; init; } = true;
    }
}