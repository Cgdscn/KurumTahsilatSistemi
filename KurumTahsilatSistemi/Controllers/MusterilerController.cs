using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MusterilerController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IAuditService _auditService;

        public MusterilerController(
            ApplicationDbContext context,
            IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? arama)
        {
            var sorgu = _context.Musteriler
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
            {
                arama = arama.Trim();

                sorgu = sorgu.Where(x =>
                    x.MusteriNo.Contains(arama) ||
                    x.AdSoyad.Contains(arama) ||
                    (x.Tckn != null &&
                     x.Tckn.Contains(arama)) ||
                    (x.VergiNo != null &&
                     x.VergiNo.Contains(arama)) ||
                    (x.Email != null &&
                     x.Email.Contains(arama)));
            }

            ViewBag.Arama = arama;

            var musteriler = await sorgu
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(musteriler);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Musteri());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Musteri model)
        {
            model.MusteriNo =
                model.MusteriNo?.Trim() ?? string.Empty;

            model.AdSoyad =
                model.AdSoyad?.Trim() ?? string.Empty;

            model.Tckn = Temizle(model.Tckn);
            model.VergiNo = Temizle(model.VergiNo);
            model.Email = Temizle(model.Email);
            model.Telefon = Temizle(model.Telefon);
            model.Adres = Temizle(model.Adres);

            if (await _context.Musteriler.AnyAsync(
                    x => x.MusteriNo == model.MusteriNo))
            {
                ModelState.AddModelError(
                    nameof(model.MusteriNo),
                    "Bu müşteri numarası daha önce kullanılmıştır.");
            }

            if (model.Tckn != null &&
                await _context.Musteriler.AnyAsync(
                    x => x.Tckn == model.Tckn))
            {
                ModelState.AddModelError(
                    nameof(model.Tckn),
                    "Bu T.C. kimlik numarası daha önce kullanılmıştır.");
            }

            if (model.VergiNo != null &&
                await _context.Musteriler.AnyAsync(
                    x => x.VergiNo == model.VergiNo))
            {
                ModelState.AddModelError(
                    nameof(model.VergiNo),
                    "Bu vergi numarası daha önce kullanılmıştır.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.OlusturmaTarihi = DateTime.Now;

            _context.Musteriler.Add(model);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                "Müşteri Oluşturma",
                $"{model.MusteriNo}");


            TempData["BasariMesaji"] =
                "Müşteri kaydı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var musteri = await _context.Musteriler
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (musteri == null)
            {
                return NotFound();
            }

            return View(musteri);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var musteri = await _context.Musteriler
                .FindAsync(id);

            if (musteri == null)
            {
                return NotFound();
            }

            return View(musteri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Musteri model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            model.MusteriNo =
                model.MusteriNo?.Trim() ?? string.Empty;

            model.AdSoyad =
                model.AdSoyad?.Trim() ?? string.Empty;

            model.Tckn = Temizle(model.Tckn);
            model.VergiNo = Temizle(model.VergiNo);
            model.Email = Temizle(model.Email);
            model.Telefon = Temizle(model.Telefon);
            model.Adres = Temizle(model.Adres);

            if (await _context.Musteriler.AnyAsync(x =>
                    x.MusteriNo == model.MusteriNo &&
                    x.Id != model.Id))
            {
                ModelState.AddModelError(
                    nameof(model.MusteriNo),
                    "Bu müşteri numarası başka bir kayıtta kullanılmaktadır.");
            }

            if (model.Tckn != null &&
                await _context.Musteriler.AnyAsync(x =>
                    x.Tckn == model.Tckn &&
                    x.Id != model.Id))
            {
                ModelState.AddModelError(
                    nameof(model.Tckn),
                    "Bu T.C. kimlik numarası başka bir kayıtta kullanılmaktadır.");
            }

            if (model.VergiNo != null &&
                await _context.Musteriler.AnyAsync(x =>
                    x.VergiNo == model.VergiNo &&
                    x.Id != model.Id))
            {
                ModelState.AddModelError(
                    nameof(model.VergiNo),
                    "Bu vergi numarası başka bir kayıtta kullanılmaktadır.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var musteri = await _context.Musteriler
                .FirstOrDefaultAsync(x => x.Id == id);

            if (musteri == null)
            {
                return NotFound();
            }

            musteri.MusteriNo = model.MusteriNo;
            musteri.AdSoyad = model.AdSoyad;
            musteri.Tckn = model.Tckn;
            musteri.VergiNo = model.VergiNo;
            musteri.Email = model.Email;
            musteri.Telefon = model.Telefon;
            musteri.Adres = model.Adres;
            musteri.AktifMi = model.AktifMi;

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Müşteri bilgileri başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        private static string? Temizle(
            string? deger)
        {
            return string.IsNullOrWhiteSpace(deger)
                ? null
                : deger.Trim();
        }
    }
}