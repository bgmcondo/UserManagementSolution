
namespace UserManagement.Application.DTOs;

// Başarılı girişte dönen JWT token bilgisi.
//   AccessToken → İstemci sonraki isteklerde "Authorization: Bearer {token}" başlığıyla gönderir.
//   ExpiresAt   → Token'ın geçerliliğinin bittiği an (UTC).
public record LoginResponseDto(string AccessToken,DateTime ExpiresAt);
