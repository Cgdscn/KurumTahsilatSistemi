using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Müşteri numarası zorunludur.")]
        [StringLength(30)]
        [Display(Name = "Müşteri Numarası")]
        public string MusteriNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "T.C. kimlik numarası zorunludur.")]
        [RegularExpression(
            @"^\d{11}$",
            ErrorMessage =
                "T.C. kimlik numarası 11 rakamdan oluşmalıdır.")]
        [Display(Name = "T.C. Kimlik Numarası")]
        public string Tckn { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [StringLength(150)]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "Parola en az 6 karakter olmalıdır.")]
        [Display(Name = "Parola")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "Parolalar eşleşmiyor.")]
        [Display(Name = "Parola Tekrarı")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}