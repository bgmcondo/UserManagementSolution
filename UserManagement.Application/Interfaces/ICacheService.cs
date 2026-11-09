namespace UserManagement.Application.Interfaces
{
    /// <summary>
    /// Uygulamanın cache işlemleri için kullandığı soyutlama.
    /// Arkada IMemoryCache veya Redis (IDistributedCache) çalışabilir;
    /// handler'lar hangisinin kullanıldığını bilmez.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Anahtara karşılık gelen değeri getirir. 
        /// </summary>
        /// <typeparam name="T">Cachedki değerin tipi</typeparam>
        /// <param name="key">Cache anahtarı(örn."users:42")</param>
        /// <param name="cancellationToken">İptal belirteci</param>
        /// <returns>Değer bulunursa kendisi, bulunamazsa default(T).</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        ///<summary>
        ///Değeri cache'e yazar. Aynı anahtar varsa üzerine yazar.
        /// </summary>
        ///<typeparam name="T">Yazılacak değerin tipi</typeparam>
        ///<param name="key">Cache anahtarı</param>
        ///<param value="value">Saklanacak değer</param>
        ///<param name="absoluteExpiration">
        ///Kesin süre dolumu. Bu süre geçince kayıt, okunsa da okunmasa da silinir.
        ///Verilmezse varsayılan süre kullanılır.</param>
        ///<param name="slidingExpiration">
        ///Kayan süre dolumu. Kayıt her okunduğunda süre sıfırlanır;
        /// bu süre boyunca okunmazsa silinir.
        /// </param>
        /// <param name="cancellationToken">İptal belirteci.</param>

        Task SetAsync<T>(
    string key,
    T value,
    TimeSpan? absoluteExpiration = null,
    TimeSpan? slidingExpiration = null,
    CancellationToken cancellationToken = default);


        ///<summary> 
        ///Değer cache'te varsa onu döner. Yoksa factory'yi çalıştırır
        ///(örn. veritabanından okur), sonucu cache'e yazar ve döner.
        /// <typeparam name="T">Değerin tipi.</typeparam>
        /// <param name="key">Cache anahtarı.</param>
        /// <param name="factory">Cache boşsa değer üretecek fonksiyon</param>
        /// <param name="absoluteExpiration">Kesin süre dolumu.</param>
        /// <param name="slidingExpiration">Kayan süre dolumu.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Cache'teki veya factory'nin ürettiği değer.</returns>
        /// </summary>
        Task<T> GetOrSetAsync<T>(
           string key,
           Func<Task<T>> factory,
           TimeSpan? absoluteExpiration = null,
           TimeSpan? slidingExpiration = null,
           CancellationToken cancellationToken = default);
        /// <summary>
        /// Anahtarın cache'te olup olmadığını kontrol eder.
        /// Kaydın süresini uzatmaz.
        /// </summary>
        /// <param name="key">Cache anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Anahtar varsa true, yoksa false.</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tek bir anahtarı cache'ten siler.
        /// Veritabanında bir kayıt güncellendikten sonra çağrılmalıdır.
        /// </summary>
        /// <param name="key">Silinecek cache anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Belirtilen önekle başlayan tüm anahtarları siler.
        /// Örneğin "users:list:" verilirse tüm kullanıcı listesi cache'leri temizlenir.
        /// </summary>
        /// <param name="prefix">Anahtar öneki.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }
}
