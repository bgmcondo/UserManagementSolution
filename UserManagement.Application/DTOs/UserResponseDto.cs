namespace UserManagement.Application.DTOs;

// API'den istemciye dönen kullanıcı bilgisi.
// Kullanıldığı yerler: GET /api/users, GET /api/users/{id}, POST /api/users yanıtı,
//                      Redis cache, SignalR admin bildirimi.
//
// AppUser'dan SADECE dışarıya gösterilmesinde sakınca olmayan alanlar alınır.
// ASLA eklenmeyecekler: PasswordHash, SecurityStamp, ConcurrencyStamp,
// NormalizedEmail, NormalizedUserName, AccessFailedCount, LockoutEnd
// ve diğer Identity iç alanları.
//
// Redis'te entity değil bu nesne saklanır; böylece cache'te de hassas veri bulunmaz.
public record UserResponseDto(
    Guid Id,
    string UserName,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Website,
    AddressDto? Address,
    CompanyDto? Company,
    DateTime CreatedAt,
    DateTime? UpdatedAt);