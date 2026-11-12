using System.Runtime.ExceptionServices;
using UserManagement.API.Hubs;
using UserManagement.API.Middleware;
using UserManagement.Application;
using UserManagement.Infrastructure;

// =====================================================================
// İstek → Middleware'ler → Controller → Servis → Veritabanı
//
// MIDDLEWARE: Her istekte otomatik çalışır. İsteği geçirir ya da durdurur (401/403).
// CONTROLLER: İsteği karşılar, servisi çağırır, cevap döner. İş mantığı yazılmaz.
// SERVİS:     Çağrılınca çalışır, asıl işi yapar.
//
// Middleware HttpContext'e yazar (ör. context.User), controller oradan okur.
// Controller'daki hata geri döner, ExceptionHandling middleware'i yakalar.
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

//servis kayıtlarını yapıyoruz burada


builder.Services.AddApplication();
//Dış dünyayla konuştuğu içi bu katman appsettingjson'dan connectionstring,eposta sunucu jwt key ve issuer vb ister
builder.Services.AddInfrastructure(builder.Configuration);

// Controller sınıflarını ([ApiController]) kullanabilmek için gerekli servisler.
builder.Services.AddControllers();

// SignalR'ı (gerçek zamanlı bildirim / WebSocket) etkinleştirir.
// Hubs klasöründeki NotificationHub bunun sayesinde çalışır.
builder.Services.AddSignalR();

// Swagger'ın endpoint'leri keşfedebilmesi için gerekli.
builder.Services.AddEndpointsApiExplorer();

// Swagger dokümanını üretir (API'yi tarayıcıdan test ettiğin ekran).
builder.Services.AddSwaggerGen();
// CORS: Farklı bir adresten (ör. React uygulaması: localhost:3000) gelen
// isteklere izin verir. Tarayıcı, varsayılan olarak başka adrese istek atmayı engeller.
builder.Services.AddCors(o => o.AddPolicy("client", p => p
    .WithOrigins("http://localhost:3000") // Sadece bu adrese izin ver
    .AllowAnyHeader()                     // Her türlü header'a izin ver (Authorization vb.)
    .AllowAnyMethod()                     // GET, POST, PUT, DELETE hepsine izin ver
    .AllowCredentials()));                // Cookie/token gönderimine izin ver. SignalR için ŞART.
                                          // Not: AllowCredentials ile AllowAnyOrigin birlikte KULLANILAMAZ,
                                          // bu yüzden adresi açıkça yazıyoruz.

var app = builder.Build(); // bu satırdan sonra servis eklenmez
//app.Use() ile başlayan her satır middleware

// Hata yakalayıcı en başta olmaöı ki alttaki herşeyin hatasını yakalayabilsin
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ortam development ortamısysa test arayüzü oluşturulsun

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // swagger.json dosyasını yayınlar
    app.UseSwaggerUI();  // /swagger adresinde test arayüzünü açar
}

//http ile gelen istekleri https e yönlendirir

app.UseHttpsRedirection();

// CORS izni. Auth'tan ÖNCE olmalı, yoksa tarayıcının token'sız OPTIONS ön isteği 401 alır.
app.UseCors("client");


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
