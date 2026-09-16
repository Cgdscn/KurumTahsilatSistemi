namespace KurumTahsilatSistemi.Services
{
    public class OdemeDogrulamaSonucu
    {
        public bool BasariliMi { get; init; }
        public string? HataMesaji { get; init; }

        public static OdemeDogrulamaSonucu Basarili()
        {
            return new OdemeDogrulamaSonucu
            {
                BasariliMi = true
            };
        }

        public static OdemeDogrulamaSonucu Basarisiz(string hataMesaji)
        {
            return new OdemeDogrulamaSonucu
            {
                BasariliMi = false,
                HataMesaji = hataMesaji
            };
        }
       
    }
}
