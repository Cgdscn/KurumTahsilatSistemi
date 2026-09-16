using KurumTahsilatSistemi.Migrations;
using KurumTahsilatSistemi.Models;

namespace KurumTahsilatSistemi.Services
{
    public class OdemeKurallariService: IOdemeKurallariService
    {
        public decimal KalanTutariHesapla(Borc borc)
        {
            ArgumentNullException.ThrowIfNull(borc);

            return Math.Max(
                0,
                borc.BorcTutari -
                borc.OdenenTutar);
        }

        public OdemeDogrulamaSonucu OdemeyiDogrula(Borc borc, decimal odemeTutari)
        { 
            ArgumentNullException.ThrowIfNull(borc);
            var kalanTutar = KalanTutariHesapla(borc);

            if(borc.Durum == "Ödendi" || kalanTutar <= 0)
            {
                return OdemeDogrulamaSonucu.Basarisiz("Bu borç daha önce tamamen ödenmiştir.");
            }

            if(odemeTutari <= 0)
            {
                return OdemeDogrulamaSonucu.Basarisiz("Ödeme tutarı sıfırdan büyük olmalıdır.");

            }

            if(odemeTutari > kalanTutar)
            {
                return OdemeDogrulamaSonucu.Basarisiz($"Ödeme tutarı kalan borç olan" + $"{kalanTutar:N2} TL'den büyük olamaz.");
            }

            return OdemeDogrulamaSonucu.Basarili();
        
        }

        public string YeniBorcDurumunuBelirle(Borc borc, decimal yeniOdemeTutari)
        {
            ArgumentNullException.ThrowIfNull(borc);

            var toplamOdenenTutar =
                borc.OdenenTutar +
                yeniOdemeTutari;

            if(toplamOdenenTutar >= borc.BorcTutari)
            {
                return "Ödendi";
            }

            if(toplamOdenenTutar > 0)
            {
                return "Kısmi Ödendi";
            }

            return "Ödenmedi";
        }
    }
}
