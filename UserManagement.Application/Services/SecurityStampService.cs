
using UserManagement.Application.Common;
using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Services
{
    public class SecurityStampService : ISecurityStampService
    {

        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;
        public SecurityStampService(ICacheService cacheService, IUserRepository userRepository) {

            _cacheService = cacheService;
            _userRepository = userRepository;


        }
        public  Task<string?> GetCurrentAsync(Guid userId, CancellationToken ct)
        {
            //Rediste varsa redisten döner
            //yoksa kullanıcıyı db den okur redise yazar
            //kullanıcı silinmişse de null döner null da cachelenemez
            return
            _cacheService.GetOrSetAsync(CacheKeys.SecurityStamp(userId),
                async () => (await _userRepository.GetByIdAsync(userId, ct))?.SecurityStamp, cancellationToken: ct);

        }
    }
    }


