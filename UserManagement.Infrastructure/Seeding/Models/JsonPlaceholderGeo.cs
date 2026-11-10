

namespace UserManagement.Infrastructure.Seeding.Models
{
    // JSON'daki "address.geo" nesnesi (konum bilgisi)
    public sealed class JsonPlaceholderGeo
    {
        public string Lat { get; init; } = string.Empty;   // Enlem
        public string Lng { get; init; } = string.Empty;   // Boylam
    }
}
