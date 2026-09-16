using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class BorcDosyaYukleViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Kurum seçilmelidir.")]
        [Display(Name = "Kurum")]
        public int KurumId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Tahsilat ürünü seçilmelidir.")]
        [Display(Name = "Tahsilat Ürünü")]
        public int TahsilatUrunuId { get; set; }

        [Required(ErrorMessage = "TXT dosyası seçilmelidir.")]
        [Display(Name = "Borç Dosyası")]
        public IFormFile? Dosya { get; set; }
    }
}