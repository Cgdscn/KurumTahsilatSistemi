using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class Kurum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurum kodu zorunludur.")]
        [StringLength(20)]
        [Display(Name = "Kurum Kodu")]
        public string KurumKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurum adı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Kurum Adı")]
        public string KurumAdi { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Kurum Hesap Numarası")]
        public string? HesapNo { get; set; }

        [Display(Name = "Aktif")]
        public bool AktifMi { get; set; } = true;

        public ICollection<TahsilatUrunu> TahsilatUrunleri { get; set; }
            = new List<TahsilatUrunu>();
    }
}
