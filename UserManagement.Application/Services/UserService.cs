using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Services
{
    //Kullanıcı işlemlerinin iş mantığı(IUserService implemantasyonu).
    //
    //Sorumlulukları:
    //- Girdiyi doğrulamak(FluentValidation)
    //-Veriyi okumak/yazmak(IUserRepository)
    // - Redis cache'i yönetmek: okurken cache-aside, yazarken invalidation (ICacheService)
    // - Yeni kullanıcıda RabbitMQ'ya event bırakmak (IPublishEndpoint)
    //- Admin'lere canlı bildirim göndermek (INotificationService)
    //tüm bağımlılıklar arayüz olarak alınır.Bu sınıf Redis'i,PostreSql'i,RabbitMQ'yu
    //veya signalR ı doğrudan bilmez testler de hrpsi mocklanabilir

    public class UserService : IUserService
    {
        public UserService()
        {
            
        }
        public Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeUserNameAsync(Guid userId, ChangeUserNameRequestDto request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponseDto> CreateAsync(CreateUserRequestDto createUserRequestDto, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
