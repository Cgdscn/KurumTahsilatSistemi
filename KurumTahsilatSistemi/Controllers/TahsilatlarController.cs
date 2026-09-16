using System.Security.Claims;
using System.Text.RegularExpressions;
using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Documents;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin,Kullanici")]
    public class TahsilatlarController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IOdemeKurallariService
            _odemeKurallariService;

        private readonly ITahsilatService
            _tahsilatService;

        private static readonly string[]
            GecerliOdemeKanallari =
        {
            "Vadesiz Hesaptan Ödeme",
            "Kredi Kartı",
            "Banka Kartı",
            "Nakit",
            "Mobil Bankacılık",
            "İnternet Bankacılığı",
            "Şube"
        };

        public TahsilatlarController(
            ApplicationDbContext context,
            IOdemeKurallariService odemeKurallariService,
            ITahsilatService tahsilatService)
        {
            _context = context;

            _odemeKurallariService =
                odemeKurallariService;

            _tahsilatService =
                tahsilatService;
        }

        // GET: Tahsilatlar/OdemeYap/5
        [HttpGet]
        public async Task<IActionResult> OdemeYap(int id)
        {
            var borc =
                await OdemeBorcuGetirAsync(id);

            if (borc is null)
            {
                return NotFound();
            }

            if (!KurumOdemeAlabilirMi(borc))
            {
                return PasifKurumYonlendirmesi();
            }

            var kalanTutar =
                borc.BorcTutari -
                borc.OdenenTutar;

            if (borc.Durum == "Ödendi" ||
                kalanTutar <= 0)
            {
                TempData["HataMesaji"] =
                    "Bu borç daha önce tamamen ödenmiştir.";

                return BorcListesineYonlendir();
            }

            var model =
                new TahsilatYapViewModel
                {
                    BorcId =
                        borc.Id,

                    MusteriAdi =
                        borc.MusteriAdi,

                    ReferansNo =
                        borc.ReferansNo,

                    BorcTutari =
                        kalanTutar
                };

            return View(model);
        }

        // POST: Tahsilatlar/OdemeYap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdemeYap(
            int BorcId,
            string? OdemeKanali)
        {
            var borc =
                await OdemeBorcuGetirAsync(
                    BorcId);

            if (borc is null)
            {
                return NotFound();
            }

            if (!KurumOdemeAlabilirMi(borc))
            {
                return PasifKurumYonlendirmesi();
            }

            var kalanTutar =
                borc.BorcTutari -
                borc.OdenenTutar;

            if (borc.Durum == "Ödendi" ||
                kalanTutar <= 0)
            {
                TempData["HataMesaji"] =
                    "Bu borç daha önce tamamen ödenmiştir.";

                return BorcListesineYonlendir();
            }

            if (string.IsNullOrWhiteSpace(
                    OdemeKanali) ||
                !GecerliOdemeKanallari.Contains(
                    OdemeKanali))
            {
                var model =
                    new TahsilatYapViewModel
                    {
                        BorcId =
                            borc.Id,

                        MusteriAdi =
                            borc.MusteriAdi,

                        ReferansNo =
                            borc.ReferansNo,

                        BorcTutari =
                            kalanTutar,

                        OdemeKanali =
                            OdemeKanali ??
                            string.Empty
                    };

                ModelState.AddModelError(
                    nameof(
                        TahsilatYapViewModel
                            .OdemeKanali),
                    "Geçerli bir ödeme seçeneği seçilmelidir.");

                return View(model);
            }

            return RedirectToAction(
                nameof(OdemeDetay),
                new
                {
                    borcId = BorcId,
                    odemeKanali = OdemeKanali
                });
        }

        // GET: Tahsilatlar/OdemeDetay
        [HttpGet]
        public async Task<IActionResult> OdemeDetay(
            int borcId,
            string odemeKanali)
        {
            var borc =
                await OdemeBorcuGetirAsync(
                    borcId);

            if (borc is null)
            {
                return NotFound();
            }

            if (!KurumOdemeAlabilirMi(borc))
            {
                return PasifKurumYonlendirmesi();
            }

            var kalanTutar =
                borc.BorcTutari -
                borc.OdenenTutar;

            if (borc.Durum == "Ödendi" ||
                kalanTutar <= 0)
            {
                TempData["HataMesaji"] =
                    "Bu borç daha önce tamamen ödenmiştir.";

                return BorcListesineYonlendir();
            }

            if (string.IsNullOrWhiteSpace(
                    odemeKanali) ||
                !GecerliOdemeKanallari.Contains(
                    odemeKanali))
            {
                TempData["HataMesaji"] =
                    "Geçersiz ödeme seçeneği.";

                return RedirectToAction(
                    nameof(OdemeYap),
                    new
                    {
                        id = borcId
                    });
            }

            var model =
                new OdemeDetayViewModel
                {
                    BorcId =
                        borc.Id,

                    MusteriAdi =
                        borc.MusteriAdi,

                    ReferansNo =
                        borc.ReferansNo,

                    BorcTutari =
                        borc.BorcTutari,

                    DahaOnceOdenenTutar =
                        borc.OdenenTutar,

                    KalanBorcTutari =
                        kalanTutar,

                    OdemeTutari =
                        kalanTutar,

                    OdemeKanali =
                        odemeKanali
                };

            return View(model);
        }

        // POST: Tahsilatlar/OdemeDetay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdemeDetay(
            OdemeDetayViewModel model)
        {
            var borc =
                await OdemeBorcuGetirAsync(
                    model.BorcId);

            if (borc is null)
            {
                return NotFound();
            }

            if (!KurumOdemeAlabilirMi(borc))
            {
                return PasifKurumYonlendirmesi();
            }

            model.MusteriAdi =
                borc.MusteriAdi;

            model.ReferansNo =
                borc.ReferansNo;

            model.BorcTutari =
                borc.BorcTutari;

            model.DahaOnceOdenenTutar =
                borc.OdenenTutar;

            model.KalanBorcTutari =
                _odemeKurallariService
                    .KalanTutariHesapla(
                        borc);

            if (borc.Durum == "Ödendi" ||
                model.KalanBorcTutari <= 0)
            {
                TempData["HataMesaji"] =
                    "Bu borç daha önce tamamen ödenmiştir.";

                return BorcListesineYonlendir();
            }

            OdemeDetaylariniDogrula(
                model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(
                    kullaniciId))
            {
                return Challenge();
            }

            /*
             * Servis çağrılmadan hemen önce
             * kurumun güncel durumu yeniden kontrol edilir.
             */
            var kurumHalaAktifMi =
                await _context.Borclar
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == model.BorcId)
                    .Select(x =>
                        x.TahsilatUrunu
                            .Kurum
                            .AktifMi)
                    .FirstOrDefaultAsync();

            if (!kurumHalaAktifMi)
            {
                return PasifKurumYonlendirmesi();
            }

            var sonuc =
                await _tahsilatService
                    .OdemeYapAsync(
                        new TahsilatOdemeTalebi
                        {
                            BorcId =
                                model.BorcId,

                            OdemeTutari =
                                model.OdemeTutari,

                            OdemeKanali =
                                model.OdemeKanali,

                            ApplicationUserId =
                                kullaniciId
                        });

            if (!sonuc.BasariliMi)
            {
                ModelState.AddModelError(
                    nameof(
                        model.OdemeTutari),
                    sonuc.Mesaj);

                var guncelBorc =
                    await OdemeBorcuGetirAsync(
                        model.BorcId);

                if (guncelBorc is not null)
                {
                    model.DahaOnceOdenenTutar =
                        guncelBorc.OdenenTutar;

                    model.KalanBorcTutari =
                        _odemeKurallariService
                            .KalanTutariHesapla(
                                guncelBorc);
                }

                return View(model);
            }

            TempData["BasariMesaji"] =
                sonuc.Mesaj;

            return RedirectToAction(
                nameof(Dekont),
                new
                {
                    id = sonuc.TahsilatId
                });
        }

        // GET: Tahsilatlar/Gecmis
        [HttpGet]
        public async Task<IActionResult> Gecmis()
        {
            var tahsilatlar =
                await YetkiliTahsilatSorgusu()
                    .OrderByDescending(x =>
                        x.IslemTarihi)
                    .ToListAsync();

            return View(tahsilatlar);
        }

        // GET: Tahsilatlar/Dekont/5
        [HttpGet]
        public async Task<IActionResult> Dekont(
            int id)
        {
            var tahsilat =
                await YetkiliTahsilatSorgusu()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);

            if (tahsilat is null)
            {
                return NotFound();
            }

            return View(tahsilat);
        }

        // GET: Tahsilatlar/DekontPdf/5
        [HttpGet]
        public async Task<IActionResult> DekontPdf(
            int id)
        {
            var tahsilat =
                await YetkiliTahsilatSorgusu()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);

            if (tahsilat is null)
            {
                return NotFound();
            }

            var document =
                new TahsilatDekontDocument(
                    tahsilat);

            var pdfBytes =
                document.GeneratePdf();

            var guvenliIslemNo =
                string.IsNullOrWhiteSpace(
                    tahsilat.IslemNo)
                    ? tahsilat.Id.ToString()
                    : tahsilat.IslemNo;

            var dosyaAdi =
                $"Tahsilat-Dekontu-" +
                $"{guvenliIslemNo}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                dosyaAdi);
        }

        // GET: Tahsilatlar/Iptal/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Iptal(
            int id)
        {
            var tahsilat =
                await _context.Tahsilatlar
                    .Include(x => x.Borc)
                    .FirstOrDefaultAsync(
                        x => x.Id == id);

            if (tahsilat is null)
            {
                return NotFound();
            }

            if (tahsilat.IslemDurumu ==
                "İptal")
            {
                TempData["HataMesaji"] =
                    "Bu tahsilat işlemi daha önce iptal edilmiştir.";

                return RedirectToAction(
                    nameof(Gecmis));
            }

            var model =
                new TahsilatIptalViewModel
                {
                    TahsilatId =
                        tahsilat.Id,

                    IslemNo =
                        tahsilat.IslemNo,

                    MusteriAdi =
                        tahsilat.Borc?
                            .MusteriAdi
                        ?? string.Empty,

                    ReferansNo =
                        tahsilat.Borc?
                            .ReferansNo
                        ?? string.Empty,

                    OdenenTutar =
                        tahsilat.OdenenTutar
                };

            return View(model);
        }

        // POST: Tahsilatlar/Iptal
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(
            TahsilatIptalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sonuc =
                await _tahsilatService
                    .TahsilatIptalEtAsync(
                        model.TahsilatId,
                        model.IptalNedeni);

            if (!sonuc.BasariliMi)
            {
                var tahsilat =
                    await _context.Tahsilatlar
                        .AsNoTracking()
                        .Include(x => x.Borc)
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                model.TahsilatId);

                if (tahsilat is not null)
                {
                    model.IslemNo =
                        tahsilat.IslemNo;

                    model.MusteriAdi =
                        tahsilat.Borc?
                            .MusteriAdi
                        ?? string.Empty;

                    model.ReferansNo =
                        tahsilat.Borc?
                            .ReferansNo
                        ?? string.Empty;

                    model.OdenenTutar =
                        tahsilat.OdenenTutar;
                }

                ModelState.AddModelError(
                    string.Empty,
                    sonuc.Mesaj);

                return View(model);
            }

            TempData["BasariMesaji"] =
                sonuc.Mesaj;

            return RedirectToAction(
                nameof(Gecmis));
        }

        /*
         * Admin için bütün tahsilatlar döndürülür.
         * Kullanıcı için yalnızca kendi tahsilatları döndürülür.
         */
        private IQueryable<Tahsilat>
            YetkiliTahsilatSorgusu()
        {
            var sorgu =
                _context.Tahsilatlar
                    .AsNoTracking()
                    .Include(x =>
                        x.ApplicationUser)
                    .Include(x =>
                        x.Borc)
                        .ThenInclude(x =>
                            x.TahsilatUrunu)
                            .ThenInclude(x =>
                                x.Kurum)
                    .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                return sorgu;
            }

            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(
                    kullaniciId))
            {
                return sorgu.Where(
                    x => false);
            }

            return sorgu.Where(
                x =>
                    x.ApplicationUserId ==
                    kullaniciId);
        }

        private void OdemeDetaylariniDogrula(
            OdemeDetayViewModel model)
        {
            if (!GecerliOdemeKanallari
                    .Contains(
                        model.OdemeKanali))
            {
                ModelState.AddModelError(
                    nameof(
                        model.OdemeKanali),
                    "Geçerli bir ödeme seçeneği seçilmelidir.");

                return;
            }

            switch (model.OdemeKanali)
            {
                case "Vadesiz Hesaptan Ödeme":
                case "Mobil Bankacılık":
                case "İnternet Bankacılığı":

                    if (string.IsNullOrWhiteSpace(
                            model.HesapNo))
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.HesapNo),
                            "Ödeme yapılacak hesap numarası girilmelidir.");
                    }

                    break;

                case "Kredi Kartı":
                case "Banka Kartı":

                    if (string.IsNullOrWhiteSpace(
                            model.KartSahibi))
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.KartSahibi),
                            "Kart üzerindeki ad ve soyad girilmelidir.");
                    }

                    var kartNo =
                        Regex.Replace(
                            model.KartNo ??
                            string.Empty,
                            @"\D",
                            string.Empty);

                    if (kartNo.Length != 16)
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.KartNo),
                            "Kart numarası 16 haneli olmalıdır.");
                    }

                    if (!Regex.IsMatch(
                            model.SonKullanmaTarihi
                                ?? string.Empty,
                            @"^(0[1-9]|1[0-2])\/\d{2}$"))
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.SonKullanmaTarihi),
                            "Son kullanma tarihi AA/YY formatında olmalıdır.");
                    }

                    if (!Regex.IsMatch(
                            model.Cvv ??
                            string.Empty,
                            @"^\d{3,4}$"))
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.Cvv),
                            "CVV 3 veya 4 haneli olmalıdır.");
                    }

                    break;

                case "Nakit":
                case "Şube":

                    if (string.IsNullOrWhiteSpace(
                            model.SubeVezneKodu))
                    {
                        ModelState.AddModelError(
                            nameof(
                                model.SubeVezneKodu),
                            "Şube veya vezne kodu girilmelidir.");
                    }

                    break;
            }
        }

        /*
         * Ödeme işlemlerinde borç, ürün ve kurum
         * bilgileri birlikte yüklenir.
         */
        private async Task<Borc?>
            OdemeBorcuGetirAsync(
                int borcId)
        {
            return await _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x =>
                    x.TahsilatUrunu)
                    .ThenInclude(x =>
                        x.Kurum)
                .FirstOrDefaultAsync(
                    x => x.Id == borcId);
        }

        /*
         * Kurum ve ürün ilişkisi mevcutsa ve kurum
         * aktifse ödeme yapılabilir.
         */
        private static bool
            KurumOdemeAlabilirMi(
                Borc borc)
        {
            return
                borc.TahsilatUrunu is not null &&
                borc.TahsilatUrunu.Kurum is not null &&
                borc.TahsilatUrunu.Kurum.AktifMi;
        }

        private IActionResult
            PasifKurumYonlendirmesi()
        {
            TempData["HataMesaji"] =
                "Bu kurum pasif durumda olduğu için " +
                "ödeme işlemi yapılamaz.";

            return BorcListesineYonlendir();
        }

        private IActionResult
            BorcListesineYonlendir()
        {
            return User.IsInRole("Admin")
                ? RedirectToAction(
                    "Sorgula",
                    "Borclar")
                : RedirectToAction(
                    "Borclarim",
                    "Borclar");
        }
    }
}