using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using UserManagement.Application.Interfaces;

public class RedisCacheService:ICacheService
{
    //Sabitler + alanlar + constructor(InstanceName, DefaultExpiration, JsonOptions, _cache, _redis)

    //aynı redisi başka  projeler de kullanabilir o yüzden önek gerek
    public const string InstanceName = "UserManagement:";

    //süresiz kayıt rediste sonsuza kalır o yüzden varsayılan olarak 10dk atıyoruz
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    //nesne ile json metni arası çeviri yapıyor. Redis nesne saklayamıyor sadece text saklayabiliyor 
    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

    //GET,SET,REMOVE,REFRESH
    private readonly IDistributedCache _cache;

    private readonly ILogger<RedisCacheService> _logger;


    //ExistsAsync,RemoveByPrefixAsync
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer,ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = connectionMultiplexer;
        _logger = logger;

    }


    // Redis kaynaklı hata mı? (bağlantı yok / zaman aşımı / sunucu hatası)
    // RedisTimeoutException, RedisException'dan değil TimeoutException'dan türediği için ikisine de bakıyoruz.
    private static bool IsRedisFailure(Exception ex) => ex is RedisException or TimeoutException;

    //SetAsync → JSON'a çevirme + süre ayarları
    public async Task SetAsync<T>(string key, T value, TimeSpan? absolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        //süre verilmediyse DefaultExpiration kullanıyoruz
        //?? soldaki null ise sağdakini al demek


        var cacheOptions = new DistributedCacheEntryOptions
        {

            AbsoluteExpirationRelativeToNow = absolute_Expiration ?? DefaultExpiration,
            // null kalırsa sliding süre kullanılmaz.
            SlidingExpiration = slidingExpiration
        };

        //Nesneyi JSON metnine çevir
        var json = JsonSerializer.Serialize(value, jsonOptions);

        // redis'e gönderiyoruz ve taskı olduğu gibi iletiyoruz
        try
        {
            await _cache.SetStringAsync(key, json, cacheOptions, cancellationToken);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            // Önbelleğe yazamadık: sorun değil, veri zaten DB'de. Sonraki okuma DB'ye gider.
            _logger.LogWarning(ex, "Redis'e yazılamadı ({Key}), önbellek atlanıyor.", key);
        }
    }
    //GetAsync → SetAsync'in tersi
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {

        //redisten json metnini oku önek otamatik olarak eklenir
        string? json;
        try
        {
            json = await _cache.GetStringAsync(key, cancellationToken);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            // Redis yoksa "önbellekte yok" say: çağıran (GetOrSetAsync vb.) DB'den okur.
            _logger.LogWarning(ex, "Redis'ten okunamadı ({Key}), veritabanına düşülüyor.", key);
            return default;
        }
        //kayıt yoksa veya süresi dolmuşsa null gelir
        if (json == null)
        {
            return default; //T neyse onun boş değeri

        }
        return JsonSerializer.Deserialize<T>(json, jsonOptions);

    }
    //RemoveAsync → tek satır
    public async Task RemoveAsync(string key,  CancellationToken cancellationToken = default)
    {

        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogWarning(ex, "Redis'ten silinemedi ({Key}).", key);
        }


    }

    //GetOrSetAsync → GetAsync + factory + SetAsync birleşimi
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        // json'u okur ve nesne olarak alır.
        var cached = await GetAsync<T>(key, cancellationToken);

        if (cached is not null)
            return cached;
        // Redis'te yok: factory'yi çalıştır (genelde veritabanı sorgusu).
        var value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, absolute_Expiration, slidingExpiration, cancellationToken);

        }

        return value;





    }
    //ExistsAsync → ilk kez _redis, önek elle eklenir
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase(); //Redise komut göndermek için
        var redisKey = InstanceName + key;
        try
        {
            return await db.KeyExistsAsync(redisKey);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogWarning(ex, "Redis'e ulaşılamadı ({Key}).", key);
            return false;
        }

    }
    //RemoveByPrefixAsync → _redis + sunucuları dolaşma + SCAN peki bunları yazarken hangi dokümandan yararlanayım?
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {

        try { 

        var db = _redis.GetDatabase();
        var redisKey = InstanceName + prefix;

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);

                //Replikalar sadece okur ; ana sunucu da silinince onlara da yansır
                if (server.IsReplica)
                {
                    continue;
                }

                //ÖNEKİ ELLE EKLE Örn: "UserManagement:users:list:*"
                var pattern = $"{InstanceName}{prefix}*";

                //KeysAync arka planda scan kullanılır:Parça parça getirir,redis'i kilitlemez.
                await foreach (var key in server.KeysAsync(pattern: pattern).WithCancellation(cancellationToken))
                    //gelen keyler zaten önekli tam ad,tekrar önek ekleme

                    await db.KeyDeleteAsync(key);


            }

            }

        catch(Exception ex)  when (IsRedisFailure(ex))
        {

            _logger.LogWarning(ex, "Redis'te '{Prefix}' önekli kayıtlar silinemedi.", prefix);


        }


        }
    }

   