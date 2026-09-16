namespace KurumTahsilatSistemi.ViewModels
{
    public class AiChatMessageViewModel
    {
        public string Rol { get; set; } = string.Empty;

        public string Icerik { get; set; } = string.Empty;

        public DateTime Zaman { get; set; } = DateTime.Now;
    }
}