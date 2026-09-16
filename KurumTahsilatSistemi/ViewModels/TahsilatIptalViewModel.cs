using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class TahsilatIptalViewModel
    {
        public int TahsilatId { get; set; }

        public string IslemNo { get; set; } = string.Empty;

        public string MusteriAdi { get; set; } = string.Empty;

        public string ReferansNo { get; set; } = string.Empty;

        public decimal OdenenTutar { get; set; }

        [Required(ErrorMessage = "İptal nedeni girilmelidir.")]
        [StringLength(250)]
        [Display(Name = "İptal Nedeni")]
        public string IptalNedeni { get; set; } = string.Empty;
    

    }
}