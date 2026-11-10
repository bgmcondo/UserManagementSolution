using FluentAssertions.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Caching
{
    /// <summary>
    /// Cache ile ilgili servisleri DI (Dependency Injection) sistemine kaydeder.
    /// Program.cs içinden builder.Services.AddCaching(builder.Configuration) ile çağrılır.
    /// </summary>
    public static class CachingRegistration
    {
        //services.AddRedisCache(Configuration) şeklinde çağrılır
        // "this IServiceCollection services": Bu bir extension metot;
        // IServiceCollection'a yeni bir metot eklemiş gibi kullanmamızı sağlar.
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //appsettingjsondan connectionstring  redis değerini oku
            var redisConnection = configuration.GetConnectionString("Redis");

            //Bağlantı adresi yoksa uygulama açılırken hata versin
            if (string.IsNullOrEmpty(redisConnection))
            {

                throw new InvalidOperationException("Redis bağlantı adresi (ConnectionStrings:Redis) bulunamadı.");

            }

            // Redis'i IDistributedCache olarak kaydet
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection; //redis şu adreste
                options.InstanceName = "UserManagement:"; //Anahtarların başına ön eki koy




            }

            );

            return services;


        }
    }
}