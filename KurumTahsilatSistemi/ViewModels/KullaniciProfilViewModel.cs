using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class KullaniciProfilViewModel
    {
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Display(Name = "Müşteri Numarası")]
        public string MusteriNo { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string Adres { get; set; } = string.Empty;

        public bool AktifMi { get; set; }

        [Display(Name = "Hesap Oluşturma Tarihi")]
        public DateTime HesapOlusturmaTarihi { get; set; }

        [Display(Name = "Toplam Ödeme Tutarı")]
        public decimal ToplamOdemeTutari { get; set; }

        [Display(Name = "Toplam Borç Tutarı")]
        public decimal ToplamBorcTutari { get; set; }

        [Display(Name = "Kalan Toplam Borç")]
        public decimal KalanToplamBorc { get; set; }

        [Display(Name = "Gecikmiş Borç Sayısı")]
        public int GecikmisBorcSayisi { get; set; }

        [Display(Name = "Ödenmemiş Borç Sayısı")]
        public int OdenmemisBorcSayisi { get; set; }
    }
}