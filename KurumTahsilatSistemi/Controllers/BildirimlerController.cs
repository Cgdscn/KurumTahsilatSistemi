using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize]
    public class BildirimlerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBildirimService _bildirimService;

        public BildirimlerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBildirimService bildirimService)
        {
            _context = context;
            _userManager = userManager;
            _bildirimService = bildirimService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            var bildirimler = await _context.Bildirimler
                .AsNoTracking()
                .Where(x =>
                    x.ApplicationUserId == user.Id)
                .OrderByDescending(x =>
                    x.OlusturmaTarihi)
                .ToListAsync();

            return View(bildirimler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OkunduIsaretle(
            int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            var bildirim = await _context.Bildirimler
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ApplicationUserId == user.Id);

            if (bildirim is null)
            {
                return NotFound();
            }

            if (!bildirim.OkunduMu)
            {
                bildirim.OkunduMu = true;
                bildirim.OkunmaTarihi = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(
                bildirim.YonlendirmeUrl) &&
                Url.IsLocalUrl(bildirim.YonlendirmeUrl))
            {
                return LocalRedirect(
                    bildirim.YonlendirmeUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TumunuOkunduIsaretle()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            await _bildirimService
                .TumunuOkunduIsaretleAsync(user.Id);

            return RedirectToAction(nameof(Index));
        }
    }
}