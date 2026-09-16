using System.Security.Claims;
using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string islemTipi,
            string aciklama,
            string durum = "Başarılı",
            string? kullanici = null)
        {
            var http =
                _httpContextAccessor.HttpContext;

            /*
             * Öncelikle dışarıdan verilen kullanıcı bilgisi alınır.
             * Bu değer e-posta, kullanıcı adı veya Identity kullanıcı ID'si olabilir.
             */
            var kullaniciBilgisi =
                kullanici?.Trim();

            /*
             * Dışarıdan kullanıcı bilgisi verilmemişse
             * oturumdaki e-posta veya kullanıcı adı kullanılır.
             */
            if (string.IsNullOrWhiteSpace(kullaniciBilgisi))
            {
                kullaniciBilgisi =
                    http?.User.FindFirstValue(
                        ClaimTypes.Email);

                kullaniciBilgisi ??=
                    http?.User.Identity?.Name;
            }

            /*
             * Gönderilen değer bir ApplicationUser ID'si ise
             * AspNetUsers tablosundan e-posta bilgisine çevrilir.
             */
            if (!string.IsNullOrWhiteSpace(kullaniciBilgisi))
            {
                var identityKullanicisi =
                    await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == kullaniciBilgisi);

                if (identityKullanicisi is not null)
                {
                    kullaniciBilgisi =
                        identityKullanicisi.Email
                        ?? identityKullanicisi.UserName
                        ?? identityKullanicisi.AdSoyad
                        ?? kullaniciBilgisi;
                }
            }

            if (string.IsNullOrWhiteSpace(kullaniciBilgisi))
            {
                kullaniciBilgisi =
                    "Bilinmeyen";
            }

            /*
             * AuditLogs.Kullanici alanı 100 karakter olduğu için
             * uzun değerlerde veri tabanı hatası oluşması engellenir.
             */
            if (kullaniciBilgisi.Length > 100)
            {
                kullaniciBilgisi =
                    kullaniciBilgisi[..100];
            }

            var log = new AuditLogs
            {
                IslemTarihi =
                    DateTime.Now,

                Kullanici =
                    kullaniciBilgisi,

                IslemTipi =
                    islemTipi,

                Aciklama =
                    aciklama,

                Durum =
                    durum,

                IPAdresi =
                    http?.Connection
                        .RemoteIpAddress?
                        .ToString(),

                Controller =
                    http?.Request
                        .RouteValues["controller"]?
                        .ToString(),

                Action =
                    http?.Request
                        .RouteValues["action"]?
                        .ToString()
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}