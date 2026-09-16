using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.Models
{
    public class Bildirim
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Baslik { get; set; }

        [Required]
        [StringLength(500)]
        public string Mesaj { get; set; }

        [Required]
        [StringLength(30)]
        public string BildirimTipi { get; set; } = "Bilgi";

        public bool OkunduMu {  get; set; } 

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime OkunmaTarihi { get; set; }

        [StringLength(200)]
        public string? YonlendirmeUrl {  get; set; }

        [Required]
        public string ApplicationUserId {  get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null;

        [Display(Name = "Borç")]
        public int? BorcId { get; set; }

        public Borc? Borc { get; set; }

        [StringLength(100)]
        [Display(Name = "Bildirim Kodu")]
        public string? BildirimKodu { get; set; }
    }
}
