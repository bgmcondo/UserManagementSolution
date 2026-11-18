using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;

namespace UserManagement.Application
{
    //Application katmanının tüm DI kayıtları burada
    // Program.cs'te builder.Services.AddApplication() ile çağrılır.
    // static: Extension metot yazabilmek için sınıf static olmalı.
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Bu projedeki TÜM validator'ları (AbstractValidator<T>'den türeyenleri) bulup kaydeder.
            // ValidationService validator'lara gevşek bağlı: somut sınıfları tanımaz, DI'dan IValidator<T> ister.
            // Kayıt olmazsa GetService null döner ve doğrulama sessizce atlanır
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Scoped: her HTTP isteği için bir örnek oluşturulur.
            // Repository ve DbContext de scoped olduğu için servisler de scoped olmalı.
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            //Validation hizmeti de kaydedilir
            services.AddScoped<IValidationService, ValidationService>();

            return services; // zincirleme çağrı için: AddApplication().AddInfrastructure(...)

        }

    }
}
