

using MassTransit;

namespace UserManagement.Infrastructure.Seeding.Models
{
    //jsonplaceholder.typicode.com/users adresinden gelen kullanıcı verisinin modelş
    //sadece veriyi taşımak için kullanılır;veritabanı entity'si değil 

    public class JsonPlaceholderUser
    {
        //Dış servisteki kullanıcı ID'si
        //bizim veritabanındaki Guid Id ile aynı şey değil
        public int Id { get; init; }

        // Ad soyad (örn. "Leanne Graham")
        public string Name { get; init; } = string.Empty;

        //kullanıcı adı(örn. "Bret")
        public string UserName {  get; init; } = string.Empty;

        //E-posta(örn."Sincere@april.biz")
       public string Email {  get; init; } = string.Empty;

        //telefon 
        public string Phone {  get; init; } = string.Empty;
        public string Website { get; internal set; }

        public JsonPlaceholderAddress Address { get; init; }

        public JsonPlaceholderCompany Company { get; init; }

        public JsonPlaceholderGeo Geo {  get; init; }
    }
}
