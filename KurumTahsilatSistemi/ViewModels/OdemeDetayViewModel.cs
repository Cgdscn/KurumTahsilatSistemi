using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class OdemeDetayViewModel
    {
        public int BorcId { get; set; }

        public string MusteriAdi { get; set; } = string.Empty;

        public string ReferansNo { get; set; } = string.Empty;

        [Display(Name = "Toplam Borç Tutarı")]
        public decimal BorcTutari { get; set; }

        [Display(Name = "Daha Önce Ödenen Tutar")]
        public decimal DahaOnceOdenenTutar { get; set; }

        [Display(Name = "Kalan Borç Tutarı")]
        public decimal KalanBorcTutari { get; set; }

        [Required(ErrorMessage = "Ödeme tutarı zorunludur.")]
        [Range(
            typeof(decimal),
            "0,01",
            "999999999,99",
            ErrorMessage = "Ödeme tutarı sıfırdan büyük olmalıdır.")]
        [Display(Name = "Ödenecek Tutar")]
        public decimal OdemeTutari { get; set; }

        [Required(ErrorMessage = "Ödeme seçeneği zorunludur.")]
        [Display(Name = "Ödeme Seçeneği")]
        public string OdemeKanali { get; set; } = string.Empty;

        [Display(Name = "Hesap Numarası")]
        public string? HesapNo { get; set; }

        [Display(Name = "Kart Üzerindeki Ad Soyad")]
        public string? KartSahibi { get; set; }

        [Display(Name = "Kart Numarası")]
        public string? KartNo { get; set; }

        [Display(Name = "Son Kullanma Tarihi")]
        public string? SonKullanmaTarihi { get; set; }

        [Display(Name = "CVV")]
        public string? Cvv { get; set; }

        [Display(Name = "Şube/Vezne Kodu")]
        public string? SubeVezneKodu { get; set; }
    }
}