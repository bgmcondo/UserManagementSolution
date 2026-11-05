namespace UserManagement.Application.DTOs;

// Kullanıcının kendi kullanıcı adını değiştirme isteği (PATCH /api/users/me/username).
//
// Id yok: kim olduğu token'dan alınır, böylece kimse başkasının adını değiştiremez.
// CurrentPassword: açık kalan bir oturumdan başkasının adı değiştirmesini engeller.
//
// Admin bu endpoint'i başkası için kullanamaz. Admin kullanıcı adını sadece
// kullanıcıyı oluştururken (CreateUserRequestDto) bir kez atar.
public record ChangeUserNameRequestDto(string NewUserName, string CurrentPassword);