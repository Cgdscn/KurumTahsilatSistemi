using KurumTahsilatSistemi.Services;

namespace KurumTahsilatSistemi.Tests.Services
{
    public class FakeAuditService : IAuditService
    {
        public Task LogAsync(
            string islemTipi,
            string aciklama,
            string durum = "Başarılı",
            string? kullanici = null)
        {
            return Task.CompletedTask;
        }
    }
}