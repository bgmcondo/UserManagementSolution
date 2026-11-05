namespace UserManagement.Application.DTOs;

// Giriş isteği (POST /api/auth/login).
public record LoginRequestDto(string UserNameOrEmail, string Password);