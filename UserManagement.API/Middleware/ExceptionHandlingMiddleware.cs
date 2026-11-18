using System.Net;
using UserManagement.Application.Exceptions;
namespace UserManagement.API.Middleware;

// Pipeline'daki tüm hataları tek bir yerde yakalayan middleware.
// Böylece controller'larda tek tek try-catch yazmana gerek kalmaz.
public class ExceptionHandlingMiddleware
{
    // Pipeline'daki bir sonraki middleware'i temsil eder
    private readonly RequestDelegate _next;

    // Hataları loglamak için. ILogger zaten ASP.NET tarafından hazır gelir.
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Her istekte otomatik çalışan metot. Adı "Invoke" veya "InvokeAsync" OLMAK ZORUNDA.
    //constructorda oluştrulan nesneleri kullananlar static olamaz

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // İsteği sıradaki middleware'e (ve sonunda controller'a) gönder
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Kullanıcı isteği iptal etti (sayfayı kapattı, bağlantı koptu).
            // Bu bir hata değil: error loglamaya ve cevap yazmaya gerek yok, kimse dinlemiyor.
            _logger.LogInformation("İstek kullanıcı tarafından iptal edildi: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            // Alttaki herhangi bir yerde hata olursa buraya düşer
            _logger.LogError(ex, "Beklenmeyen bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Hata türüne göre uygun HTTP durum kodunu seç
        var statusCode = exception switch
        {
            ValidationException=>HttpStatusCode.BadRequest,//  400: Geçersiz veri / yanlış mevcut parola
            KeyNotFoundException => HttpStatusCode.NotFound,            // 404: Kayıt bulunamadı
            ArgumentException => HttpStatusCode.BadRequest,          // 400: Hatalı veri
            ForbiddenException=>HttpStatusCode.Forbidden, // yetki yok
            UnauthorizedException => HttpStatusCode.Unauthorized,        // 401: Yetkisiz
            ConflictException=>HttpStatusCode.Conflict, //  409: Aynı e - posta / kullanıcı adı


            _ => HttpStatusCode.InternalServerError  // 500: Diğer tüm hatalar
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // 500 hatalarında gerçek mesajı kullanıcıya gösterme (güvenlik açığı olabilir)
        var message = statusCode == HttpStatusCode.InternalServerError
            ? "Sunucuda bir hata oluştu."
            : exception.Message;

        //  Doğrulama hatalarında hangi alanın neden hatalı olduğunu da gönder
        // Örn: "errors": { "Email": ["Geçerli bir e-posta adresi girilmelidir."] }
        // Diğer hatalarda null döner.
        var errors = exception is ValidationException ve && ve.Errors.Count > 0
            ? ve.Errors
            : null;

        // Frontend'e düzenli bir JSON döndür
        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = (int)statusCode,
            message,
            errors
        });
    }
}