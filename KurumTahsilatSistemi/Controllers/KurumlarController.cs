using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KurumlarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KurumlarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kurumlar
        public async Task<IActionResult> Index()
        {
            var kurumlar = await _context.Kurumlar
                .AsNoTracking()
                .OrderBy(x => x.KurumAdi)
                .ToListAsync();

            return View(kurumlar);
        }

        // GET: Kurumlar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurumlar
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (kurum is null)
            {
                return NotFound();
            }

            return View(kurum);
        }

        // GET: Kurumlar/Create
        public IActionResult Create()
        {
            return View(new Kurum
            {
                AktifMi = true
            });
        }

        // POST: Kurumlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,KurumKodu,KurumAdi,HesapNo,AktifMi")]
            Kurum kurum)
        {
            kurum.KurumKodu = kurum.KurumKodu?.Trim();
            kurum.KurumAdi = kurum.KurumAdi?.Trim();
            kurum.HesapNo = kurum.HesapNo?.Trim();

            // Kurum kodu kontrolü
            if (!string.IsNullOrWhiteSpace(kurum.KurumKodu))
            {
                var kurumKoduVarMi = await _context.Kurumlar
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.KurumKodu == kurum.KurumKodu);

                if (kurumKoduVarMi)
                {
                    ModelState.AddModelError(
                        nameof(kurum.KurumKodu),
                        "Bu kurum kodu başka bir kurum tarafından kullanılmaktadır.");
                }
            }

            // Hesap numarası kontrolü
            if (!string.IsNullOrWhiteSpace(kurum.HesapNo))
            {
                var hesapNoVarMi = await _context.Kurumlar
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.HesapNo == kurum.HesapNo);

                if (hesapNoVarMi)
                {
                    ModelState.AddModelError(
                        nameof(kurum.HesapNo),
                        "Bu hesap numarası başka bir kurum tarafından kullanılmaktadır.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(kurum);
            }

            try
            {
                _context.Kurumlar.Add(kurum);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Kurum başarıyla oluşturuldu.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Kurum kaydedilirken bir veritabanı hatası oluştu.");

                return View(kurum);
            }
        }

        // GET: Kurumlar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurumlar
                .FindAsync(id.Value);

            if (kurum is null)
            {
                return NotFound();
            }

            return View(kurum);
        }

        // POST: Kurumlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,KurumKodu,KurumAdi,HesapNo,AktifMi")]
            Kurum kurum)
        {
            if (id != kurum.Id)
            {
                return NotFound();
            }

            kurum.KurumKodu = kurum.KurumKodu?.Trim();
            kurum.KurumAdi = kurum.KurumAdi?.Trim();
            kurum.HesapNo = kurum.HesapNo?.Trim();

            // Başka kurumda aynı kurum kodu var mı?
            if (!string.IsNullOrWhiteSpace(kurum.KurumKodu))
            {
                var kurumKoduVarMi = await _context.Kurumlar
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id != kurum.Id &&
                        x.KurumKodu == kurum.KurumKodu);

                if (kurumKoduVarMi)
                {
                    ModelState.AddModelError(
                        nameof(kurum.KurumKodu),
                        "Bu kurum kodu başka bir kurum tarafından kullanılmaktadır.");
                }
            }

            // Başka kurumda aynı hesap numarası var mı?
            if (!string.IsNullOrWhiteSpace(kurum.HesapNo))
            {
                var hesapNoVarMi = await _context.Kurumlar
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id != kurum.Id &&
                        x.HesapNo == kurum.HesapNo);

                if (hesapNoVarMi)
                {
                    ModelState.AddModelError(
                        nameof(kurum.HesapNo),
                        "Bu hesap numarası başka bir kurum tarafından kullanılmaktadır.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(kurum);
            }

            try
            {
                _context.Kurumlar.Update(kurum);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Kurum bilgileri başarıyla güncellendi.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KurumExists(kurum.Id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Kurum güncellenirken bir veritabanı hatası oluştu.");

                return View(kurum);
            }
        }

        // GET: Kurumlar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurumlar
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (kurum is null)
            {
                return NotFound();
            }

            return View(kurum);
        }

        // POST: Kurumlar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kurum = await _context.Kurumlar
                .FindAsync(id);

            if (kurum is null)
            {
                return NotFound();
            }

            try
            {
                _context.Kurumlar.Remove(kurum);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Kurum başarıyla silindi.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Bu kuruma bağlı kayıtlar bulunduğu için kurum silinemedi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KurumExists(int id)
        {
            return _context.Kurumlar
                .Any(x => x.Id == id);
        }
    }
}