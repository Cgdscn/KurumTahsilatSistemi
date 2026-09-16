using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class DosyaAktarim
    {
        public int Id { get; set; }

        public int KurumId { get; set; }

        public Kurum Kurum { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Display(Name = "Dosya Adı")]
        public string DosyaAdi { get; set; } = string.Empty;

        [Display(Name = "Yükleme Tarihi")]
        public DateTime YuklemeTarihi { get; set; }

        [Display(Name = "Toplam Kayıt")]
        public int ToplamKayitSayisi { get; set; }

        [Display(Name = "Başarılı Kayıt")]
        public int BasariliKayitSayisi { get; set; }

        [Display(Name = "Hatalı Kayıt")]
        public int HataliKayitSayisi { get; set; }

        [Required]
        [StringLength(30)]
        public string Durum { get; set; } = "İşleniyor";
    }
}
