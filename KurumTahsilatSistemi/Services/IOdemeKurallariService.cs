using KurumTahsilatSistemi.Models;

namespace KurumTahsilatSistemi.Services
{
    public interface IOdemeKurallariService
    {
        decimal KalanTutariHesapla(Borc borc);

        OdemeDogrulamaSonucu OdemeyiDogrula(
            Borc borc,
            decimal odemeTutari);

        string YeniBorcDurumunuBelirle(
            Borc borc,
            decimal yeniOdemeTutari);
    }
}
