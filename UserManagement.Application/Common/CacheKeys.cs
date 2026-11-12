namespace UserManagement.Application.Common
{
    public static class CacheKeys
    {
        /// <summary>
        /// Tüm kullanıcı listesi cache'lerinin ortak öneki.
        /// Bir kullanıcı eklendiğinde, güncellendiğinde veya silindiğinde
        /// RemoveByPrefixAsync(CacheKeys.UserListPrefix) ile tüm listeler birden temizlenir.
        /// </summary>
        
        public const string  UserListPrefix= "users:list:"; //constlar otomatik static

    
         /// tek bir Kullanıcının Id değeri
        /// <returns>Örnek: "users:42"</returns>
       public static string User(Guid id) => $"users:{id}";

        //bütün kullanıcılar
        public const string AllUsers = UserListPrefix + "all";

    }
}
