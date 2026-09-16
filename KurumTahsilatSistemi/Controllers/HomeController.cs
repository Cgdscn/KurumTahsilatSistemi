using System.Diagnostics;
using System.Security.Claims;
using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize(Roles = "Admin,Kullanici")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var adminMi =
                User.IsInRole("Admin");

            var bugun =
                DateTime.Today;

            var yarin =
                bugun.AddDays(1);

            var ayinIlkGunu =
                new DateTime(
                    bugun.Year,
                    bugun.Month,
                    1);

            var sonYediGunBaslangici =
                bugun.AddDays(-6);

            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!adminMi &&
                string.IsNullOrWhiteSpace(kullaniciId))
            {
                return Challenge();
            }

            /*
             * Tahsilat sorgusu iliþkili borç, kurum,
             * ürün ve kullanýcý bilgileriyle hazýrlanýr.
             */
            var tahsilatSorgusu =
                _context.Tahsilatlar
                    .AsNoTracking()
                    .Include(x => x.ApplicationUser)
                    .Include(x => x.Borc)
                        .ThenInclude(x => x.TahsilatUrunu)
                            .ThenInclude(x => x.Kurum)
                    .AsQueryable();

            /*
             * Normal kullanýcý yalnýzca kendi
             * gerçekleþtirdiði tahsilatlarý görür.
             */
            if (!adminMi)
            {
                tahsilatSorgusu =
                    tahsilatSorgusu.Where(x =>
                        x.ApplicationUserId ==
                        kullaniciId);
            }

            /*
             * Baþarýlý tahsilatlar için ortak sorgu.
             * Ýptal edilmiþ iþlemler hesaplamalara katýlmaz.
             */
            var basariliTahsilatSorgusu =
                tahsilatSorgusu.Where(x =>
                    x.IslemDurumu == "Baþarýlý");

            var model =
                new DashboardViewModel
                {
                    AktifKurumSayisi =
                        adminMi
                            ? await _context.Kurumlar
                                .AsNoTracking()
                                .CountAsync(x =>
                                    x.AktifMi)
                            : 0,

                    TahsilatUrunuSayisi =
                        adminMi
                            ? await _context.TahsilatUrunleri
                                .AsNoTracking()
                                .CountAsync()
                            : 0,

                    ToplamBorcSayisi =
                        adminMi
                            ? await _context.Borclar
                                .AsNoTracking()
                                .CountAsync()
                            : 0,

                    OdenmemisBorcSayisi =
                        adminMi
                            ? await _context.Borclar
                                .AsNoTracking()
                                .CountAsync(x =>
                                    x.Durum == "Ödenmedi")
                            : 0,

                    OdenmisBorcSayisi =
                        adminMi
                            ? await _context.Borclar
                                .AsNoTracking()
                                .CountAsync(x =>
                                    x.Durum == "Ödendi")
                            : 0,

                    KismiOdenenBorcSayisi =
                        adminMi
                            ? await _context.Borclar
                                .AsNoTracking()
                                .CountAsync(x =>
                                    x.Durum == "Kýsmi Ödendi")
                            : 0,

                    AktifMusteriSayisi =
                        adminMi
                            ? await _context.Musteriler
                                .AsNoTracking()
                                .CountAsync(x =>
                                    x.AktifMi)
                            : 0,

                    BasariliTahsilatSayisi =
                        await basariliTahsilatSorgusu
                            .CountAsync(),

                    ToplamTahsilatTutari =
                        await basariliTahsilatSorgusu
                            .SumAsync(x =>
                                (decimal?)x.OdenenTutar)
                        ?? 0,

                    BugunkuTahsilatTutari =
                        await basariliTahsilatSorgusu
                            .Where(x =>
                                x.IslemTarihi >= bugun &&
                                x.IslemTarihi < yarin)
                            .SumAsync(x =>
                                (decimal?)x.OdenenTutar)
                        ?? 0,

                    BuAykiTahsilatTutari =
                        await basariliTahsilatSorgusu
                            .Where(x =>
                                x.IslemTarihi >= ayinIlkGunu &&
                                x.IslemTarihi < yarin)
                            .SumAsync(x =>
                                (decimal?)x.OdenenTutar)
                        ?? 0,

                    SonTahsilatlar =
                        await tahsilatSorgusu
                            .OrderByDescending(x =>
                                x.IslemTarihi)
                            .Take(5)
                            .ToListAsync()
                };

            /*
             * Gecikmiþ borçlarýn kalan tutarlarý hesaplanýr.
             *
             * KalanTutar NotMapped olduðu için doðrudan
             * EF sorgusu içinde kullanýlamaz. Bu nedenle
             * BorcTutari ve OdenenTutar alanlarý çekilerek
             * bellekte hesaplama yapýlýr.
             */
            if (adminMi)
            {
                var gecikmisBorcVerileri =
                    await _context.Borclar
                        .AsNoTracking()
                        .Where(x =>
                            x.Durum != "Ödendi" &&
                            x.SonOdemeTarihi < bugun)
                        .Select(x => new
                        {
                            x.BorcTutari,
                            x.OdenenTutar
                        })
                        .ToListAsync();

                model.GecikmisBorcTutari =
                    gecikmisBorcVerileri.Sum(x =>
                        Math.Max(
                            0,
                            x.BorcTutari -
                            x.OdenenTutar));
            }

            /*
             * Son yedi gündeki baþarýlý tahsilatlar alýnýr.
             * Veri bulunmayan günler grafikte sýfýr gösterilir.
             */
            var sonYediGunTahsilatlari =
                await basariliTahsilatSorgusu
                    .Where(x =>
                        x.IslemTarihi >=
                            sonYediGunBaslangici &&
                        x.IslemTarihi < yarin)
                    .Select(x => new
                    {
                        x.IslemTarihi,
                        x.OdenenTutar
                    })
                    .ToListAsync();

            for (var i = 0; i < 7; i++)
            {
                var tarih =
                    sonYediGunBaslangici
                        .AddDays(i);

                var gunlukToplam =
                    sonYediGunTahsilatlari
                        .Where(x =>
                            x.IslemTarihi.Date ==
                            tarih.Date)
                        .Sum(x =>
                            x.OdenenTutar);

                model.SonYediGunEtiketleri.Add(
                    tarih.ToString("dd.MM"));

                model.SonYediGunTahsilatlari.Add(
                    gunlukToplam);
            }

            /*
             * Baþarýlý tahsilatlar ödeme kanallarýna
             * göre gruplandýrýlýr.
             */
            var odemeKanaliDagilimi =
                await basariliTahsilatSorgusu
                    .GroupBy(x =>
                        x.OdemeKanali)
                    .Select(grup => new
                    {
                        OdemeKanali =
                            grup.Key,

                        ToplamTutar =
                            grup.Sum(x =>
                                x.OdenenTutar)
                    })
                    .OrderByDescending(x =>
                        x.ToplamTutar)
                    .ToListAsync();

            model.OdemeKanaliEtiketleri =
                odemeKanaliDagilimi
                    .Select(x =>
                        string.IsNullOrWhiteSpace(
                            x.OdemeKanali)
                            ? "Belirtilmedi"
                            : x.OdemeKanali)
                    .ToList();

            model.OdemeKanaliTutarlari =
                odemeKanaliDagilimi
                    .Select(x =>
                        x.ToplamTutar)
                    .ToList();

            /*
             * Borç durum grafiði yalnýzca admin
             * dashboard'unda kullanýlacaktýr.
             */
            if (adminMi)
            {
                model.BorcDurumEtiketleri =
                    new List<string>
                    {
                        "Ödenmedi",
                        "Kýsmi Ödendi",
                        "Ödendi"
                    };

                model.BorcDurumSayilari =
                    new List<int>
                    {
                        model.OdenmemisBorcSayisi,
                        model.KismiOdenenBorcSayisi,
                        model.OdenmisBorcSayisi
                    };
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }

        [AllowAnonymous]
        [Route("Home/HttpDurumu/{code:int}")]
        public IActionResult HttpDurumu(int code)
        {
            Response.StatusCode = code;

            ViewBag.StatusCode = code;

            return View();
        }
    }
}