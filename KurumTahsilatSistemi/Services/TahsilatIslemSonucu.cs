using DocumentFormat.OpenXml.Wordprocessing;

namespace KurumTahsilatSistemi.Services
{
    public class TahsilatIslemSonucu
    {
        public bool BasariliMi { get; init; }
        public string Mesaj { get; init; } = string.Empty;
        public int? TahsilatId { get; init; }
        public static TahsilatIslemSonucu Basarili(
            string mesaj,
            int? tahsilatId = null)
        {
            return new TahsilatIslemSonucu
            {
                BasariliMi = true,
                Mesaj = mesaj,
                TahsilatId = tahsilatId
            };
        }

        public static TahsilatIslemSonucu Basarisiz(
            string mesaj)
        {
            return new TahsilatIslemSonucu
            {
                BasariliMi = false,
                Mesaj = mesaj
            };
        }
    }
}
