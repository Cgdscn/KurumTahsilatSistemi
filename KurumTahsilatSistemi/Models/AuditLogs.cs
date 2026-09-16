using System.ComponentModel.DataAnnotations;
namespace KurumTahsilatSistemi.Models
{
    public class AuditLogs
    {
        public int Id { get; set; }

        [Required]
        public DateTime IslemTarihi { get; set; }


        [Required]
        [StringLength(100)]
        public string Kullanici { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string IslemTipi { get; set; } = "";

        [Required]
        [StringLength(500)]
        public string Aciklama { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string IPAdresi {  get; set; }

       
        [StringLength(100)]
        public string Controller {get; set; }

        [StringLength(100)]
        public string Action { get; set; }

        public string Durum { get; set; } = "Başarılı";


    }
}
