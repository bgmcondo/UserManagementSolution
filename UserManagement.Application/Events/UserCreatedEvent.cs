namespace UserManagement.Application.Events;

// "Bir kullanıcı oluşturuldu" olayı.
//
// UserService kullanıcıyı kaydettikten sonra bu event'i MassTransit ile RabbitMQ'ya bırakır.
// UserCreatedConsumer (Infrastructure) kuyruğu dinler ve hoş geldin e-postasını gönderir.
// Yayınlayan (UserService) ile dinleyen (Consumer) birbirini tanımaz.
//
// İsimlendirme: Event'ler GEÇMİŞ ZAMANLA adlandırılır (UserCreated, OrderShipped),
// çünkü olmuş bir şeyi bildirir. "CreateUser" ise bir emir (command) olurdu.
//
// İçerik: Sadece dinleyenlerin işine yarayacak alanlar.
//   - Parola ASLA olmaz: mesaj RabbitMQ'da saklanır, loglanabilir, _error kuyruğuna düşebilir.
//   - UserResponseDto'nun tamamı gönderilmez: e-posta için adres veya şirket gerekmez.
//     Mesaj küçük kalır ve event, API DTO'su değiştiğinde etkilenmez.
public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime CreatedAt);