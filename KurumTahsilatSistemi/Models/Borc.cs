using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KurumTahsilatSistemi.Models
{
    public class Borc
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Referans Numarası")]
        public string ReferansNo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Müşteri Numarası")]
        public string MusteriNo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Müşteri Adı")]
        public string MusteriAdi { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Borç Tutarı")]
        public decimal BorcTutari { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ödenen Tutar")]

        public decimal OdenenTutar { get; set; } = 0;

        [NotMapped]
        [Display(Name = "Kalan Tutar")]
        public decimal KalanTutar =>
            Math.Max(0, BorcTutari - OdenenTutar);

        [Display(Name = "Son Ödeme Tarihi")]
        public DateTime SonOdemeTarihi { get; set; }

        [Required]
        [StringLength(20)]
        public string Durum { get; set; } = "Ödenmedi";

        [Display(Name = "Tahsilat Ürünü")]
        public int TahsilatUrunuId { get; set; }

        public TahsilatUrunu TahsilatUrunu { get; set; } = null!;

        public ICollection<Tahsilat> Tahsilatlar { get; set; }
            = new List<Tahsilat>();

        [Display(Name = "Musteri")]
        public int? MusteriId { get; set; }
        public Musteri? Musteri { get; set; }

        [NotMapped]
        [Display(Name = "Gecikmiş Borç")]
        public bool GecikmedeMi =>
            Durum != "Ödendi " &&
            SonOdemeTarihi.Date < DateTime.Now;

        [NotMapped]
        [Display(Name = "Geciken Gün Sayısı")]
        public int GecikenGunSayisi =>
            GecikmedeMi
             ? (DateTime.Today - SonOdemeTarihi.Date).Days
            : 0;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        

    }
}
