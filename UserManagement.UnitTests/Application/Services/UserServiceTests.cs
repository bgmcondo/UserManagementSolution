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
using Xunit;


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
                _notificationServiceMock.Object);

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
        public async Task GetAllAsync_CacheMiss_ReturnsUsersFromRepository() {

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
            var result=await _service.GetAllAsync(CancellationToken.None);

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
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

        }



        // ---------- YARDIMCILAR ----------
        private static AppUser CreateUser() => new()
        {
            Id = Guid.NewGuid(),
            UserName = "ayse",
            Email = "ayse@test.com",
            FullName = "Ayşe"
        };

    }
}
