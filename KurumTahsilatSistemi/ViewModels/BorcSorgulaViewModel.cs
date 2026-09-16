using KurumTahsilatSistemi.Models;
using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class BorcSorgulaViewModel
    {
        [Display(Name = "Müşteri Numarası")]
        public string? MusteriNo { get; set; }

        [Display(Name = "Referans Numarası")]
        public string? ReferansNo { get; set; }
        public int? KurumId { get; set; }

        public List<Borc> Borclar { get; set; } = new();

        public List<Kurum> Kurumlar { get; set; }
           = new();
    }
}