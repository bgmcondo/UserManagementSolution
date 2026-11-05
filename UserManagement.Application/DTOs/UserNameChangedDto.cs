namespace UserManagement.Application.DTOs;

// Kullanıcı adı değiştiğinde Admin'lere SignalR ile gönderilen bildirim.
public record UserNameChangedDto(Guid UserId, string OldUserName, string NewUserName, DateTime ChangedAt);