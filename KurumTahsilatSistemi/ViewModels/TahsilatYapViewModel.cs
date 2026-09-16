using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class TahsilatYapViewModel
    {
        public int BorcId { get; set; }

        [Display(Name = "Müşteri Adı")]
        public string MusteriAdi { get; set; } = string.Empty;

        [Display(Name = "Referans Numarası")]
        public string ReferansNo { get; set; } = string.Empty;

        [Display(Name = "Borç Tutarı")]
        public decimal BorcTutari { get; set; }

        [Required(ErrorMessage = "Ödeme kanalı seçilmelidir.")]
        [Display(Name = "Ödeme Kanalı")]
        public string OdemeKanali { get; set; } = string.Empty;


        [Display(Name = "Toplam Borç")]
        public decimal ToplamBorcTutari { get; set; }

        [Display(Name = "Daha Önce Ödenen")]
        public decimal DahaOnceOdenenTutar { get; set; }

        [Display(Name = "Kalan Borç")]
        public decimal KalanBorcTutari { get; set; }

        [Required(ErrorMessage = "Ödeme Tutarı Zorunludur.")]
        [Range(
            typeof(decimal),
            "0,01",
            "999999999,99",
             ErrorMessage = "Ödeme tutarı sıfırdan büyük olmalıdır.")]
        [Display(Name = "Ödenecek Tutar")]
        public decimal OdemeTutari { get; set; }

    }
}