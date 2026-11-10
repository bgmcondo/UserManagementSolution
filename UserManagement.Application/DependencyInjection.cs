using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application
{
    //Application katmanının tüm DI kayıtları burada
    // Program.cs'te builder.Services.AddApplication() ile çağrılır.
    // static: Extension metot yazabilmek için sınıf static olmalı.
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication()
        {



        }

    }
}
