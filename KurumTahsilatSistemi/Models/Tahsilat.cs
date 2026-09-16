using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KurumTahsilatSistemi.Models
{
    public class Tahsilat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "İşlem Numarası")]
        public string IslemNo { get; set; } = string.Empty;

        public int BorcId { get; set; }

        public Borc Borc { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ödenen Tutar")]
        public decimal OdenenTutar { get; set; }

        [Display(Name = "İşlem Tarihi")]
        public DateTime IslemTarihi { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Ödeme Kanalı")]
        public string OdemeKanali { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "İşlem Durumu")]
        public string IslemDurumu { get; set; } = "Başarılı";

        [StringLength(250)]
        [Display(Name = "İptal Nedeni")]
        public string? IptalNedeni { get; set; }

        [Display(Name = "İptal Tarihi")]
        public DateTime? IptalTarihi { get; set; }

        // Tahsilatı gerçekleştiren Identity kullanıcısının Id değeri
        [Display(Name = "İşlemi Yapan Kullanıcı")]
        public string? ApplicationUserId { get; set; }

        // Identity kullanıcı ilişkisi
        public ApplicationUser? ApplicationUser { get; set; }
    }
}