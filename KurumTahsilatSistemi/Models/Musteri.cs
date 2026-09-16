using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class Musteri
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Müşteri numarası zorunludur.")]
        [StringLength(30)]
        [Display(Name = "Müşteri Numarası")]
        public string MusteriNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad soyad veya unvan zorunludur.")]
        [StringLength(120)]
        [Display(Name = "Ad Soyad / Unvan")]
        public string AdSoyad { get; set; } = string.Empty;

        [StringLength(11)]
        [RegularExpression(
            @"^\d{11}$",
            ErrorMessage = "T.C. kimlik numarası 11 rakamdan oluşmalıdır.")]
        [Display(Name = "T.C. Kimlik Numarası")]
        public string? Tckn { get; set; }

        [StringLength(10)]
        [RegularExpression(
            @"^\d{10}$",
            ErrorMessage = "Vergi numarası 10 rakamdan oluşmalıdır.")]
        [Display(Name = "Vergi Numarası")]
        public string? VergiNo { get; set; }

        [EmailAddress(
            ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(150)]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Phone(
            ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [StringLength(250)]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Display(Name = "Aktif")]
        public bool AktifMi { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public ICollection<Borc> Borclar {  get; set; } = new List<Borc>();
    }
}