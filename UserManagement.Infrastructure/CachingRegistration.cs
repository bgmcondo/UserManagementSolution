using FluentAssertions.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using UserManagement.Application.Interfaces;
using UserManagement.Infrastructure.Caching;

namespace UserManagement.Infrastructure
{
    /// <summary>
    /// Cache ile ilgili servisleri DI (Dependency Injection) sistemine kaydeder.
    /// Program.cs içinden builder.Services.AddCaching(builder.Configuration) ile çağrılır.
    /// </summary>
    public static class CachingRegistration
    {

        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            // appsettings.json içindeki "ConnectionStrings:Redis" değerini oku.
            // Örn: "localhost:6379"
            var redisConnection = configuration.GetConnectionString("Redis");

            // Bağlantı metni yoksa uygulama açılırken hemen hata ver.
            // Böylece sorun ilk istekte değil, en başta fark edilir.
            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new InvalidOperationException(
                    "Redis bağlantı metni bulunamadı. appsettings.json içine ConnectionStrings:Redis ekleyin.");
            }
            // 1) Redis'e doğrudan bağlantı (ConnectionMultiplexer).
            // Singleton: uygulama boyunca tek bir bağlantı açılır ve herkes onu paylaşır.
            // RedisCacheService bunu ExistsAsync ve RemoveByPrefixAsync için kullanıyor.
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnection));
            // 2) IDistributedCache'in Redis versiyonu.
            // RedisCacheService bunu Get, Set ve Remove için kullanıyor.
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;

                // Anahtarların başına eklenecek önek.
                // RedisCacheService'teki sabitle aynı olmalı, yoksa ExistsAsync ve
                // RemoveByPrefixAsync yanlış anahtarları arar. Bu yüzden sabiti doğrudan kullanıyoruz.
                options.InstanceName = RedisCacheService.InstanceName;  });
            // 3) Bizim servisimiz.
            // Biri ICacheService istediğinde RedisCacheService verilir.
            // Singleton: içinde sadece paylaşılan bağlantılar var, her istek için yenisini oluşturmaya gerek yok.
            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
    }
}