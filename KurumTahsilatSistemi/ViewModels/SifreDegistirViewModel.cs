using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class SifreDegistirViewModel
    {
        [Required(
            ErrorMessage = "Mevcut parola zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Parola")]
        public string MevcutParola { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Yeni parola zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "Yeni parola en az 6 karakter olmalıdır.")]
        [Display(Name = "Yeni Parola")]
        public string YeniParola { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Yeni parola tekrar zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(YeniParola),
            ErrorMessage =
                "Yeni parola ve parola tekrarı eşleşmiyor.")]
        [Display(Name = "Yeni Parola Tekrar")]
        public string YeniParolaTekrar { get; set; }
            = string.Empty;
    }
}