using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Infrastructure.Seeding
{
    //Seeding ayarları
    //değerleri appsettingjsondaki seed bölümünden okur

     public sealed class SeedOptions
    {
        // appsettings.json'daki bölüm adı
        public const string SectionName = "Seed";

        // Seeding çalışsın mı?
        // Geliştirmede true, canlı ortamda false yapılır.
        public bool Enabled { get; init; } = true;

        // Kullanıcıların çekileceği adres
        public string UsersUrl { get; init; } = "https://jsonplaceholder.typicode.com/users";

        // Eklenen kullanıcılara verilecek varsayılan şifre.
        // jsonplaceholder şifre vermediği için, bu kullanıcılarla
        // login olabilmek adına bir şifre belirliyoruz.
        public string DefaultPassword { get; init; } = string.Empty;
        public List<AdminSeedOptions> Admins { get; init; } = [];

           public sealed class AdminSeedOptions
        {

            // Admin'in kullanıcı adı
            public string AdminUserName { get; init; } = string.Empty;

            // Admin'in e-postası (admin var mı kontrolü de bununla yapılır)
            public string AdminEmail { get; init; } = string.Empty;

            //Fullname yazmadığımız adminlere varsayılan "System Administrator" olarak verilir.
            public string FullName { get; init; } = "System Administrator";

            // Admin'in şifresi
            public string AdminPassword { get; init; } = string.Empty;

        }
    }
}
