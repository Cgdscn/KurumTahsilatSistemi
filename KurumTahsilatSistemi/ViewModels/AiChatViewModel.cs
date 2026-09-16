using System.ComponentModel.DataAnnotations;

namespace KurumTahsilatSistemi.ViewModels
{
    public class AiChatViewModel
    {
        [Required(ErrorMessage = "Lütfen bir mesaj yazınız.")]
        [StringLength(
            500,
            ErrorMessage = "Mesaj en fazla 500 karakter olabilir.")]
        [Display(Name = "Mesajınız")]
        public string Mesaj { get; set; } = string.Empty;

        public List<AiChatMessageViewModel> Mesajlar { get; set; }
            = new();
    }
}