// Şirket bilgisi. Domain'deki Company value object'inin DTO karşılığı.
// Tüm alanlar isteğe bağlı (nullable), çünkü şirket bilgisi zorunlu değil.
//
//   Name        → Şirket adı.                          Örnek: "Romaguera-Crona"
//   CatchPhrase → Şirket sloganı.                      Örnek: "Multi-layered client-server neural-net"
//   Bs          → Faaliyet alanını anlatan iş jargonu.  Örnek: "harness real-time e-markets"
//                 Alan adı kaynak veriden (jsonplaceholder) olduğu gibi alındı.
public record CompanyDto(string? Name, string? CatchPhrase, string? Bs);