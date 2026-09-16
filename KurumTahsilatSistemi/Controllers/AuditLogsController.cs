using KurumTahsilatSistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {

            _context = context;
        }

        public async Task<IActionResult> Index(
            string? kullanici,
            string? islemTipi)
        {
            var sorgu = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(kullanici))
            {
                sorgu = sorgu.Where(x => 
                x.Kullanici.Contains(kullanici));
            }

            if (!string.IsNullOrWhiteSpace(islemTipi)) { 

                sorgu = sorgu .Where(x =>
                x.IslemTipi.Contains(islemTipi));
            
            }

            var liste = await sorgu 
                .OrderByDescending(x => x.IslemTarihi)
                .ToListAsync();

            return View(liste);
        }
        
    }
}
