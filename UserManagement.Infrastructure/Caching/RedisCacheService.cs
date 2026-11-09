using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

public class RedisCacheService
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


    //ExistsAsync,RemoveByPrefixAsync
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache;
        _redis = connectionMultiplexer;

    }

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
        await _cache.SetStringAsync(key, json, cacheOptions, cancellationToken);

    }
    //GetAsync → SetAsync'in tersi
    public async Task<T?> GetTaskAsync<T>(string key, T value, TimeSpan? absolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absolute_Expiration ?? DefaultExpiration,
            SlidingExpiration = slidingExpiration
        };
        //redisten json metnini oku önek otamatik olarak eklenir
        var json = await _cache.GetStringAsync(key, cancellationToken);
        //kayıt yoksa veya süresi dolmuşsa null gelir
        if (json == null)
        {
            return default; //T neyse onun boş değeri

        }
        return JsonSerializer.Deserialize<T>(json, jsonOptions);

    }
    //RemoveAsync → tek satır
    public async Task RemoveAsync(string key, TimeSpan? absolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);


    }

    //GetOrSetAsync → GetAsync + factory + SetAsync birleşimi
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(key, cancellationToken);

        if (json == null) { return default; }
        // Redis'te yok: factory'yi çalıştır (genelde veritabanı sorgusu).
        var value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, absolute_Expiration, slidingExpiration, cancellationToken);

        }

        return value;





    }
    //ExistsAsync → ilk kez _redis, önek elle eklenir
    public Task<bool> ExistsAsync(string key, TimeSpan? ablsolute_Expiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase(); //Redise komut göndermek için
        var redisKey = InstanceName + key;
        return db.KeyExistsAsync(redisKey);

    }
    //RemoveByPrefixAsync → _redis + sunucuları dolaşma + SCAN peki bunları yazarken hangi dokümandan yararlanayım?
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {

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

            {


            }



        }
    }

}