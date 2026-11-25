namespace UserManagement.Application.Common
{
    // Token'a koyduğumuz, standart olmayan claim adları
    public static class CustomClaimTypes
    {
        // Token üretildiği andaki SecurityStamp. Her istekte güncel değerle karşılaştırılır.
        public const string SecurityStamp = "sstamp";
    }
}