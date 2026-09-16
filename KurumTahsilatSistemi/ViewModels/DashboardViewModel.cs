using KurumTahsilatSistemi.Models;

namespace KurumTahsilatSistemi.ViewModels
{
    public class DashboardViewModel
    {
        public int AktifKurumSayisi { get; set; }

        public int TahsilatUrunuSayisi { get; set; }

        public int ToplamBorcSayisi { get; set; }

        public int OdenmemisBorcSayisi { get; set; }

        public int OdenmisBorcSayisi { get; set; }

        public int BasariliTahsilatSayisi { get; set; }

        public decimal ToplamTahsilatTutari { get; set; }

        public List<Tahsilat> SonTahsilatlar { get; set; } = new();

        public decimal BugunkuTahsilatTutari { get; set; }

        public decimal BuAykiTahsilatTutari { get; set; }

        public decimal GecikmisBorcTutari { get; set; }

        public int AktifMusteriSayisi { get; set; }

        public int KismiOdenenBorcSayisi { get; set; }

        public List<string> SonYediGunEtiketleri { get; set; }
            = new();

        public List<decimal> SonYediGunTahsilatlari { get; set; }
            = new();

        public List<string> OdemeKanaliEtiketleri { get; set; }
            = new();

        public List<decimal> OdemeKanaliTutarlari { get; set; }
            = new();

        public List<string> BorcDurumEtiketleri { get; set; }
            = new();

        public List<int> BorcDurumSayilari { get; set; }
            = new();
    }
}