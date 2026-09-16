using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin,Kullanici")]
    public class BorclarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorclarController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /*
         * Borç sorgulama ekranı yalnızca Admin tarafından
         * kullanılacaksa bu action'a Admin rolü eklenebilir.
         */
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Sorgula(
    BorcSorgulaViewModel model)
        {
            /*
             * Sayfa ilk açıldığında bütün aktif borçlar alınır.
             *
             * Aktif borç:
             * - Tamamen ödenmemiş
             * - Kısmi ödenmiş
             * - Kalan tutarı sıfırdan büyük
             */
            var sorgu = _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .Where(x =>
                    x.Durum != "Ödendi" &&
                    x.BorcTutari > x.OdenenTutar)
                .AsQueryable();

            /*
             * Müşteri numarası girilmişse yalnızca
             * ilgili müşterinin borçları gösterilir.
             */
            if (!string.IsNullOrWhiteSpace(
                    model.MusteriNo))
            {
                var musteriNo =
                    model.MusteriNo.Trim();

                sorgu = sorgu.Where(x =>
                    x.Musteri != null &&
                    x.Musteri.MusteriNo.Contains(
                        musteriNo));
            }

            /*
             * Referans numarası isteğe bağlı filtredir.
             */
            if (!string.IsNullOrWhiteSpace(
                    model.ReferansNo))
            {
                var referansNo =
                    model.ReferansNo.Trim();

                sorgu = sorgu.Where(x =>
                    x.ReferansNo.Contains(
                        referansNo));
            }

            /*
             * Kurum filtresi uygulanır.
             */
            if (model.KurumId.HasValue)
            {
                sorgu = sorgu.Where(x =>
                    x.TahsilatUrunu.KurumId ==
                    model.KurumId.Value);
            }

            model.Borclar = await sorgu
                .OrderBy(x => x.SonOdemeTarihi)
                .ThenBy(x =>
                    x.Musteri != null
                        ? x.Musteri.MusteriNo
                        : string.Empty)
                .ToListAsync();

            model.Kurumlar = await _context.Kurumlar
                .AsNoTracking()
                .Where(x => x.AktifMi)
                .OrderBy(x => x.KurumAdi)
                .ToListAsync();

            return View(model);
        }

        

        [Authorize(Roles = "Kullanici")]
        [HttpGet]
        public async Task<IActionResult> Borclarim()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            if (user.MusteriId is null)
            {
                TempData["HataMesaji"] =
                    "Kullanıcı hesabınız bir müşteri kaydıyla " +
                    "ilişkilendirilmemiştir.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var borclar = await _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .Where(x =>
                    x.MusteriId == user.MusteriId)
                .OrderBy(x =>
                    x.Durum == "Ödendi")
                .ThenBy(x =>
                    x.SonOdemeTarihi)
                .ToListAsync();

            return View(borclar);
        }
    }
}