namespace UserManagement.Application.DTOs;

// Kullanıcı güncelleme isteği (PUT /api/users/{id}).
//
// CreateUserRequest'ten farkları:
//   - Id yok        → URL'den gelir (/api/users/{id})
//   - UserName yok  → kullanıcı adı bu endpoint'te değiştirilemez
//   - Password yok  → parola değişikliği ayrı bir işlem olmalı
//                     (eski parola doğrulaması vb. gerektirir)
//
// PUT olduğu için tam güncelleme yapılır: Address veya Company null gönderilirse
// mevcut bilgi silinir.
// hem user kendini hem admin userları güncelleyebilir
public record UpdateUserRequestDto(
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Website,
    AddressDto? Address,
    CompanyDto? Company);