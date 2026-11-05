namespace UserManagement.Application.DTOs;

// Kullanıcının kendi parolasını değiştirme isteği (PATCH /api/users/me/password).
//
// Id yok: kim olduğu token'dan alınır, böylece kimse başkasının parolasını değiştiremez.
// CurrentPassword: değişikliği onaylamak için. Açık kalan bir oturumdan başkası parolayı değiştiremez.
// NewPassword: parola kurallarına uymalı (PasswordRules.ValidPassword).
public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);