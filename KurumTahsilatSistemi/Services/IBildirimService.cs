namespace KurumTahsilatSistemi.Services
{
    public interface IBildirimService
    {
        Task BildirimOlusturAsync(
            string applicationUserId,
            string baslik,
            string mesaj,
            string bildirimTipi = "Bilgi",
            string? yonlendirmeUrl = null);

        Task MusteriyeBildirimOlusturAsync(
            int musteriId,
            string baslik,
            string mesaj,
            string bildirimTipi = "Bilgi",
            string? yonlendirmeUrl = null);

        Task<int> OkunmamisBildirimSayisiAsync(
            string applicationUserId);

        Task TumunuOkunduIsaretleAsync(
            string applicationUserId);
    }
}