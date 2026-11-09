namespace UserManagement.Application.Common
{
    public static class CacheKeys
    {
        /// <summary>
        /// Tüm kullanıcı listesi cache'lerinin ortak öneki.
        /// Bir kullanıcı eklendiğinde, güncellendiğinde veya silindiğinde
        /// RemoveByPrefixAsync(CacheKeys.UserListPrefix) ile tüm listeler birden temizlenir.
        /// </summary>
        
        public const string  UserListPrefix= "users:list:";

        /// <summary>
        /// Tek bir kullanıcının cache anahtarını üretir.
        /// </summary>
        /// <param name="id">Kullanıcının Id değeri.</param>
        /// <returns>Örnek: "users:42"</returns>
        public static string User(int id) => $"users:{id}";

        ///<summary>
        ///
        /// </summary>
    }
}
