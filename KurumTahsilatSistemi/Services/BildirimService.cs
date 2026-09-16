using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Services
{
    public class BildirimService : IBildirimService
    {
        private readonly ApplicationDbContext _context;

        public BildirimService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task BildirimOlusturAsync(
            string applicationUserId,
            string baslik,
            string mesaj,
            string bildirimTipi = "Bilgi",
            string? yonlendirmeUrl = null)
        {
            if (string.IsNullOrWhiteSpace(applicationUserId))
            {
                return;
            }

            var kullaniciVarMi =
                await _context.Users.AnyAsync(x =>
                    x.Id == applicationUserId);

            if (!kullaniciVarMi)
            {
                return;
            }

            var bildirim = new Bildirim
            {
                ApplicationUserId = applicationUserId,
                Baslik = baslik.Trim(),
                Mesaj = mesaj.Trim(),
                BildirimTipi = bildirimTipi,
                YonlendirmeUrl = yonlendirmeUrl,
                OkunduMu = false,
                OlusturmaTarihi = DateTime.Now
            };

            _context.Bildirimler.Add(bildirim);

            await _context.SaveChangesAsync();
        }

        public async Task MusteriyeBildirimOlusturAsync(
            int musteriId,
            string baslik,
            string mesaj,
            string bildirimTipi = "Bilgi",
            string? yonlendirmeUrl = null)
        {
            var kullanici = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MusteriId == musteriId);

            if (kullanici is null)
            {
                /*
                 * Müşterinin henüz Identity hesabı yoksa
                 * bildirim oluşturulmaz.
                 */
                return;
            }

            await BildirimOlusturAsync(
                kullanici.Id,
                baslik,
                mesaj,
                bildirimTipi,
                yonlendirmeUrl);
        }

        public async Task<int> OkunmamisBildirimSayisiAsync(
            string applicationUserId)
        {
            return await _context.Bildirimler
                .AsNoTracking()
                .CountAsync(x =>
                    x.ApplicationUserId == applicationUserId &&
                    !x.OkunduMu);
        }

        public async Task TumunuOkunduIsaretleAsync(
            string applicationUserId)
        {
            var bildirimler = await _context.Bildirimler
                .Where(x =>
                    x.ApplicationUserId == applicationUserId &&
                    !x.OkunduMu)
                .ToListAsync();

            if (bildirimler.Count == 0)
            {
                return;
            }

            foreach (var bildirim in bildirimler)
            {
                bildirim.OkunduMu = true;
                bildirim.OkunmaTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}