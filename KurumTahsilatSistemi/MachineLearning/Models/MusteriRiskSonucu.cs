namespace KurumTahsilatSistemi.MachineLearning.Models
{
    public class MusteriRiskSonucu
    {
        public int MusteriId { get; set; }

        public string MusteriNo { get; set; }
            = string.Empty;

        public string AdSoyad { get; set; }
            = string.Empty;

        public string MusteriTipi { get; set; }
            = string.Empty;

        public int ToplamBorcSayisi { get; set; }

        public int OdenmemisBorcSayisi { get; set; }

        public int ZamanindaOdenenBorcSayisi { get; set; }

        public int GecikmeliOdenenBorcSayisi { get; set; }

        public decimal ToplamOdenmemisTutar { get; set; }

        public double GecmisZamanindaOdemeOrani { get; set; }

        public double TahminiZamanindaOdemeOlasiligi { get; set; }

        public string RiskSeviyesi { get; set; }
            = string.Empty;

        public bool TahminYapildiMi { get; set; }

        public string? Aciklama { get; set; }

        public List<BorcRiskSonucu> BorcRiskleri { get; set; }
            = new();
    }

    public class BorcRiskSonucu
    {
        public int BorcId { get; set; }

        public string ReferansNo { get; set; }
            = string.Empty;

        public string KurumAdi { get; set; }
            = string.Empty;

        public string UrunAdi { get; set; }
            = string.Empty;

        public decimal BorcTutari { get; set; }

        public DateTime SonOdemeTarihi { get; set; }

        public double ZamanindaOdemeOlasiligi { get; set; }

        public string RiskSeviyesi { get; set; }
            = string.Empty;
    }
}
