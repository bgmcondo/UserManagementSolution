namespace UserManagement.Application.DTOs;

// Yeni kullanıcı ekleme isteği (POST /api/users).
//
// Zorunlu: UserName, Email, Password, FullName
// İsteğe bağlı: PhoneNumber, Website, Address, Company
//
// OLMAMASI gerekenler:
//   - Id        → sistem üretir
//   - Role      → sistem "User" atar; istemci kendine rol veremez
//   - CreatedAt → sistem atar
//
// Password düz metin gelir, Identity tarafından hash'lenerek saklanır.
// UserName'de "@" olamaz (CreateUserRequestValidator), çünkü login ekranında
// "@" içeren değer e-posta olarak yorumlanıyor.
public record CreateUserRequestDto(
    string UserName,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    string? Website,
    AddressDto? Address,
    CompanyDto? Company);