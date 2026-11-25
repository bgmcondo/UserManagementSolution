using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Extensions
{
    // Controller kaydı ve [ApiController] davranış ayarları (Program.cs'i sade tutmak için)
    public static class ControllerExtensions
    {
        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                // [ApiController] istek controller'a ulaşmadan reddederse varsayılan ProblemDetails yerine
                // middleware'le aynı formatta cevap dön. Örnekler:
                //   Bozuk JSON:   { "email": "a@b.com",      (kapanış parantezi yok)
                //   Yanlış tür:   { "email": 123 }           (string beklenirken sayı)
                //   Boş body:     POST /api/users, body boş


                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Where(e => e.Value!.Errors.Count > 0)
                    .ToDictionary(e => e.Key, e=>e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());


                    return new BadRequestObjectResult
                   (
                        new
                        {
                            statusCode = StatusCodes.Status400BadRequest,
                            message = "Bir veya daha fazla doğrulama hatası oluştu.",
                            errors
                        });
                }









                );
                 return services;
        }
    }
}