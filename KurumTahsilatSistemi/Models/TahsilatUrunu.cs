using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class TahsilatUrunu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün kodu zorunludur.")]
        [StringLength(20)]
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Kurum seçilmelidir.")]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        public Kurum? Kurum { get; set; }

        public ICollection<Borc> Borclar { get; set; }
            = new List<Borc>();
    }
}