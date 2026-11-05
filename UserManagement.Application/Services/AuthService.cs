using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Services
{
 

    public class AuthService : IAuthService
    
    {
        public Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
