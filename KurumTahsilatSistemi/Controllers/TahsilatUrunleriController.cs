using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TahsilatUrunleriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TahsilatUrunleriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TahsilatUrunleri
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TahsilatUrunleri.Include(t => t.Kurum);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TahsilatUrunleri/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tahsilatUrunu = await _context.TahsilatUrunleri
                .Include(t => t.Kurum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tahsilatUrunu == null)
            {
                return NotFound();
            }

            return View(tahsilatUrunu);
        }

        // GET: TahsilatUrunleri/Create
        public IActionResult Create()
        {
            ViewData["KurumId"] = new SelectList(_context.Kurumlar, "Id", "KurumAdi");
            return View();
        }

        // POST: TahsilatUrunleri/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UrunKodu,UrunAdi,KurumId")] TahsilatUrunu tahsilatUrunu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tahsilatUrunu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KurumId"] = new SelectList(_context.Kurumlar, "Id", "KurumAdi", tahsilatUrunu.KurumId);
            return View(tahsilatUrunu);
        }

        // GET: TahsilatUrunleri/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tahsilatUrunu = await _context.TahsilatUrunleri.FindAsync(id);
            if (tahsilatUrunu == null)
            {
                return NotFound();
            }
            ViewData["KurumId"] = new SelectList(_context.Kurumlar, "Id", "KurumAdi", tahsilatUrunu.KurumId);
            return View(tahsilatUrunu);
        }

        // POST: TahsilatUrunleri/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UrunKodu,UrunAdi,KurumId")] TahsilatUrunu tahsilatUrunu)
        {
            if (id != tahsilatUrunu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tahsilatUrunu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TahsilatUrunuExists(tahsilatUrunu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KurumId"] = new SelectList(_context.Kurumlar, "Id", "KurumAdi", tahsilatUrunu.KurumId);
            return View(tahsilatUrunu);
        }

        // GET: TahsilatUrunleri/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tahsilatUrunu = await _context.TahsilatUrunleri
                .Include(t => t.Kurum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tahsilatUrunu == null)
            {
                return NotFound();
            }

            return View(tahsilatUrunu);
        }

        // POST: TahsilatUrunleri/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tahsilatUrunu = await _context.TahsilatUrunleri.FindAsync(id);
            if (tahsilatUrunu != null)
            {
                _context.TahsilatUrunleri.Remove(tahsilatUrunu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TahsilatUrunuExists(int id)
        {
            return _context.TahsilatUrunleri.Any(e => e.Id == id);
        }
    }
}
