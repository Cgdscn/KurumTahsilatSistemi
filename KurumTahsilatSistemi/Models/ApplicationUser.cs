using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad soyad alanı zorunludur.")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public int? MusteriId { get; set; }

        public Musteri? Musteri { get; set; }

        public ICollection<Bildirim> Bildirimler {  get; set; } = new List<Bildirim>();
    }
}