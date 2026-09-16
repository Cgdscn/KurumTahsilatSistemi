using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.ViewModels;

namespace KurumTahsilatSistemi.Services
{
    public interface IAiChatService
    {
        Task<string> CevaplaAsync(
            List<AiChatMessageViewModel> mesajlar,
            AiKullaniciBaglami kullaniciBaglami,
            CancellationToken cancellationToken);
    }
}