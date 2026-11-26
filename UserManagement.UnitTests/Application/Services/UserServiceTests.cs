// FluentAssertions: Sonuçları okunaklı şekilde kontrol etmemizi sağlar.
//   Örnek: result.Should().Be(5)  →  "sonuç 5 olmalı"
//   Şimdilik kullanılmıyor, ilk testte devreye girecek.
using FluentAssertions;
// Moq: Sahte (mock) nesne üretmemizi sağlar. Mock<...> buradan gelir.
using Moq;
using System.Net.WebSockets;
using UserManagement.Application.Common;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Mappings;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Application.Exceptions;
using Xunit;
using Microsoft.AspNetCore.Identity;



namespace UserManagement.UnitTests.Application.Services
{
    public class UserServiceTests
    {
        //user service constructorında bu 4 bağımlılığı istiyor
        //Bu bağımlılıklar olmadan biz UserService oluşturamayız
        // Gerçeklerini verirsek PostgreSQL, Redis, SignalR gerekir.
        //Bu yüzden her birinin sahte versiyonunu oluşturduk

        // Moq, arayüzdeki tüm metotları bizim yerimize içi boş olarak üretir.
        // Biz bir şey söylemedikçe bu metotlar varsayılan değer döner:
        //   nesne → null,  bool → false
        //
        // Her mock'un iki yüzü var:
        //   _repository         → KUMANDA. Testte biz kullanırız: "şunu döndür", "çağrıldı mı?"
        //   _repository.Object  → SAHTE NESNENİN KENDİSİ. UserService bunu kullanır.

        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly Mock<IJwtTokenService> _tokenServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();

        // ---------- TEST EDİLEN SINIF ----------
        //
        // Bu GERÇEK UserService. Mock<UserService> DEĞİL.
        // Sınıftaki tek gerçek nesne bu, etrafındaki her şey sahte.
        private readonly UserService _service;

        public UserServiceTests()
        {
            // Gerçek UserService'i oluşturup içine sahte bağımlılıkları veriyoruz.
            // .Object veriyoruz çünkü UserService "kumanda"yı değil, IUserRepository
            // gibi davranan nesneyi istiyor.
            _service = new UserService(
                _userRepositoryMock.Object,
                _cacheServiceMock.Object,
                _validationServiceMock.Object,
                _notificationServiceMock.Object,
                _tokenServiceMock.Object                );

        }

        [Fact] //bu attribute XUNİT'e bu metot bir test çalıştır der

        //rediste liste varsa onu olduğu gibi dönüyor,veritabanına hiç gitmiyor

        public async Task GetAllAsync_CacheHit_ReturnsCachedListWithoutCallingRepository()
        {
            // ---------- Arrange ----------
            // Cache'te varmış gibi davranacak liste (DTO'yu mapping ile üretiyoruz)

            var cachedUsers = new List<UserResponseDto>
            {
                 new AppUser { Id = Guid.NewGuid(), UserName = "ayse", Email = "ayse@test.com", FullName = "Ayşe" }
            .ToResponseDto()

            };

            //Kumandaya talimat AllUsers anahtarıyla GetAsync çağrılırsa bu listeyi döndür
            _cacheServiceMock.Setup(c => c.GetAsync<List<UserResponseDto>>(CacheKeys.AllUsers, It.IsAny<CancellationToken>()))
        .ReturnsAsync(cachedUsers);

            // ---------- Act ----------
            var result = await _service.GetAllAsync(CancellationToken.None);

            // ---------- Assert ----------
            // 1) Dönen liste cache'teki listenin ta kendisi olmalı
            result.Should().BeSameAs(cachedUsers);

            // 2) Repository'ye hiç dokunulmamalı
            _userRepositoryMock.VerifyNoOtherCalls();

            // 3) Cache'te sadece GetAsync çağrılmalı, SetAsync çağrılmamalı
            _cacheServiceMock.Verify(
                c => c.GetAsync<List<UserResponseDto>>(CacheKeys.AllUsers, It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheServiceMock.VerifyNoOtherCalls();

        }

        [Fact]
        public async Task GetAllAsync_CacheMiss_ReturnsUsersFromRepository()
        {

            //Arrange
            //Cache için setup yok
            //mock null döner, yani cache boş

            var user = CreateUser();

            // Sahte repository'ye talimat: "GetAllAsync çağrılırsa bu listeyi döndür".
            // Setup olmasaydı Moq varsayılan değeri (null ya da boş liste) döndürürdü;
            // servis ya NullReferenceException ile patlar ya da boş liste dönerdi.
            //   It.IsAny<CancellationToken>() → hangi token gelirse gelsin talimat geçerli
            //   ReturnsAsync                  → metot Task döndürdüğü için sonucu Task'a sarar
            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppUser> { user });

            //Act
            //Gerçek servisi çağırıyoruz
            //cache boş olduğu için servis repoya gidecek
            //setupda verdiğimiz listeyi alıp Dto ya çevirecek
            var result = await _service.GetAllAsync(CancellationToken.None);

            //Assert 
            //Repositorye bir kullanıcı verdik 
            result.Should().ContainSingle();

            // Dönen DTO gerçekten bizim kullanıcımızdan üretilmiş olmalı
            // (Email benzersiz olduğu için karşılaştırmada onu kullanıyoruz)
            result[0].Email.Should().Be(user.Email);

        }


        //Bu test cache boşken servisin Dbden çektiği bilgiyi cache'de yazıp yazmadadığını kontrol eder
        [Fact]
        public async Task GetAllAsync_CacheMiss_WritesResultToCache()
        {
            // Arrange
            // Önceki testle aynı: cache için Setup yok → cache boş,
            // repository bir kullanıcı döndürüyor

            var user = CreateUser();
            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppUser> { user });
            // Act
            await _service.GetAllAsync(CancellationToken.None);
            // Assert
            // SetAsync, AllUsers anahtarıyla ve bizim kullanıcımızı içeren listeyle
            // tam bir kez çağrılmış olmalı.
            //   It.Is<...>(l => ...)  → parametre bu koşula uyuyorsa eşleşir
            //   It.IsAny<TimeSpan?>() → süre parametresi ne olursa olsun fark etmez
            _cacheServiceMock.Verify(
                c => c.SetAsync(
                    CacheKeys.AllUsers,
                    It.Is<List<UserResponseDto>>(l => l.Count == 1 && l[0].Email == user.Email),
                    It.IsAny<TimeSpan?>(),      // 3 absoluteExpiration

                    It.IsAny<TimeSpan?>(),     // 4 slidingExpiration
                    It.IsAny<CancellationToken>()),
                Times.Once);

        }


        //Kullanıcı veritabanında varsa servis onu DTO'ya çevirip dönmeli
        [Fact]
        public async Task GetByIdAsync_UserExists_ReturnsDto()
        {
            //Arrange
            var user = CreateUser();

            //Talimat tam olarak bu id ile sorulursa kullanıcıyı döndürür
            // It.IsAny<CancellationToken>() → token ne olursa olsun talimat geçerli
            // (token'ın değeri bu testin konusu değil)
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            //Act
            var result = await _service.GetByIdAsync(user.Id, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Should().Be(user.Email);
        }

        //Kullnıcı yoksa servis null dönmemeli NotFoundException Fırlatmalı

        [Fact]
        public async Task GetByIdAsync_UserNotFoundThrowsNotFoundException()
        {
            //arrange :Test için gereken her şeyi kurulduğu kısım: sahte veri, mock talimatları
            // Setup YOK → Moq GetByIdAsync için null döner, yani "kullanıcı yok"
            var id = Guid.NewGuid();

            //Act
            //metodu hemen çalıştırmıyoruz bir değişkene koyuyoruz
            //çünkü hata  fırlatacak;FluentAssertions onu yakalayıp kontrol edecek
            Func<Task> act = () => _service.GetByIdAsync(id, CancellationToken.None);


            // Assert
            // "Bu çalışınca NotFoundException fırlatmalı"
            await act.Should().ThrowAsync<NotFoundException>();

        }
        //Aynı eposta kayıtlıysa ConflictException(409) fırlatmalı
        //kullanıcı oluşturulmamalı


        [Fact]

        public async Task CreateAsync_EmailAlreadyExists_ThrowsConflictException()
        {
            var request = CreateRequest();

            //Bu epostayla sorulursa zaten bir kullanıcı var diyoruz

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser());

            //Act 
            Func<Task> act = () => _service.CreateAsync(request, CancellationToken.None);

            //assert

            await act.Should().ThrowAsync<ConflictException>();

            //Sahte repository, defterine bak: gerçek servis senin CreateAsync metodunu hiç çağırdı mı? Sıfır kere olmalı.

            // Hata fırladıysa repository'nin CreateAsync'i HİÇ çağrılmamalı
            //   It.IsAny<AppUser>() → hangi kullanıcı nesnesi olursa olsun
            //   Times.Never         → hiç çağrılmamış olmalı
            //Bu kodun amacı zaten bu emailin kaydı var tekrardan oluşturulmadığı  doğrulansın
            _userRepositoryMock.Verify(
                r => r.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

        }


        // Her şey yolundaysa: kullanıcı "User" rolüyle oluşturulmalı,
        // liste cache'i temizlenmeli ve adminlere bildirim gitmeli
        [Fact]
        public async Task CreateAsync_ValidRequest_CreateUser_ClearsCacheAndNotifies()
        {
            //Arrange
            var request = CreateRequest();
            // GetByEmailAsync için Setup YOK → null döner → e-posta boşta
            // Repository'ye: "CreateAsync çağrılırsa başarılı sonuç döndür"
            _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<AppUser>(), request.Password, "User", It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);

            //Act
            var result = await _service.CreateAsync(request, CancellationToken.None);

            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.UserName.Should().Be(request.UserName);

            //verify işlemi
            //kullanıcı bir kere kaydedilmiş olmalı 

            _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AppUser>(), request.Password, "User", It.IsAny<CancellationToken>()), Times.Once);

            //liste cacheleri temizlenmiş olmalı 
            _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.UserListPrefix, It.IsAny<CancellationToken>()), Times.Once);

            //Adminlere  bu kullanıcı için bildirim gitmiş olmalı
            _notificationServiceMock.Verify(n => n.NotifyUserCreatedAsync(It.Is<UserResponseDto>(u => u.Email == request.Email), It.IsAny<CancellationToken>()), Times.Once);
        }



        //Kullanıcı yoksa:NotFoundException fırlatmalı hiçbir şey kaydedilmemeli
        [Fact]
        public async Task UpdateAsync_UserNotFound_ThrowsNotFoundExceptionAsync()
        {
            // Arrange
            // GetByIdAsync için Setup YOK → null döner → kullanıcı yok
            var id = Guid.NewGuid();

            var request = UpdateRequest();
            //act

            Func<Task> Act = () => _service.UpdateAsync(id, request, CancellationToken.None);

            //assert
            await Act.Should().ThrowAsync<NotFoundException>();

            //Kullanıcı bulunamadıysa güncellenme hiç denenmemeli

            _userRepositoryMock.Verify( r => r.UpdateAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);


        }


        //Herşey yolundaysa:Yeni değerler kullanıcıya aktarılmalı,kaydedilmeli
        //Hem kullanıcının kendi cache'i hem de liste cacheleri temizlenmeli
        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesUserAndClearsCache()
        {
            //Arrange
            var user=CreateUser();
            var request = UpdateRequest();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id,It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _userRepositoryMock.Setup(r => r.UpdateAsync(user,It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);

            //Act
            var result = await _service.UpdateAsync(user.Id, request, CancellationToken.None);

            //Assert
            //Dönen Dto yeni bilgileri taşımalı 
            result.Email.Should().Be(request.Email);
            result.FullName.Should().Be(request.FullName);
            result.PhoneNumber.Should().Be(request.PhoneNumber);

            //Kaydedilen kullanıcı listesinde yeni eposta olmalı 
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<AppUser>(u=>u.Email==request.Email),It.IsAny<CancellationToken>()),Times.Once);

            //Bu kullanıcının tek kayıt cache 



        }






        // ---------- YARDIMCILAR ----------
        private static AppUser CreateUser() => new()
        {
            Id = Guid.NewGuid(),
            UserName = "ayse",
            Email = "ayse@test.com",
            FullName = "Ayşe"
        };

        private static CreateUserRequestDto CreateRequest() => new(
           UserName: "ayse",
           Email: "ayse@test.com",
           Password: "Test123!",
           FullName: "Ayşe",
           PhoneNumber: null,
           Website: null,
           Address: null,
           Company: null);


        private static UpdateUserRequestDto UpdateRequest() => new(
           Email: "yeni@test.com",
           FullName: "Ayşe Yeni",
           PhoneNumber: "555-1234",
           Website: null,
           Address: null,
           Company: null);

    }
}
