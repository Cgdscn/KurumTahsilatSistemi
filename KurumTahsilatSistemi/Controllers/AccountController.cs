using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    

namespace KurumTahsilatSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;


        private readonly ApplicationDbContext _context;

        private readonly IAuditService _auditService;

        private readonly IVadeBildirimService _vadeBildirimService;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IAuditService auditService,
            IVadeBildirimService vadeBildirimService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _auditService = auditService;
            _vadeBildirimService = vadeBildirimService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }
            return View(new RegisterViewModel());

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            var musteri = user.MusteriId.HasValue
                ? await _context.Musteriler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == user.MusteriId.Value)
                : null;

            /*
             * Admin hesabının bağlı müşteri kaydı
             * olmayabilir. Bu durumda ödeme ve borç
             * değerleri sıfır olarak gösterilir.
             */
            var borclar = musteri is null
                ? new List<Borc>()
                : await _context.Borclar
                    .AsNoTracking()
                    .Where(x =>
                        x.MusteriId == musteri.Id)
                    .ToListAsync();

            var toplamOdemeTutari =
                await _context.Tahsilatlar
                    .AsNoTracking()
                    .Where(x =>
                        x.ApplicationUserId == user.Id &&
                        x.IslemDurumu == "Başarılı")
                    .SumAsync(x =>
                        (decimal?)x.OdenenTutar)
                ?? 0;

            var bugun = DateTime.Today;
            var kullaniciAktifMi = user.AktifMi;


            var musteriAktifMi = musteri?.AktifMi ?? true;


            var model = new KullaniciProfilViewModel
            {
                AdSoyad =
                    user.AdSoyad,

                MusteriNo =
                    musteri?.MusteriNo
                    ?? "-",

                Email =
                    user.Email
                    ?? "-",

                Telefon =
                    musteri?.Telefon
                    ?? "-",

                Adres =
                    musteri?.Adres
                    ?? "-",
                AktifMi =
                     kullaniciAktifMi &&
                     musteriAktifMi,


                HesapOlusturmaTarihi =
                    user.OlusturmaTarihi,

                ToplamOdemeTutari =
                    toplamOdemeTutari,

                ToplamBorcTutari =
                    borclar.Sum(x =>
                        x.BorcTutari),

                KalanToplamBorc =
                    borclar.Sum(x =>
                        Math.Max(
                            0,
                            x.BorcTutari -
                            x.OdenenTutar)),

                OdenmemisBorcSayisi =
                    borclar.Count(x =>
                        x.Durum != "Ödendi"),

                GecikmisBorcSayisi =
                    borclar.Count(x =>
                        x.Durum != "Ödendi" &&
                        x.SonOdemeTarihi.Date <
                            bugun),
                

            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult SifreDegistir()
        {
            return View(
                new SifreDegistirViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(
            SifreDegistirViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            var sonuc =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.MevcutParola,
                    model.YeniParola);

            if (!sonuc.Succeeded)
            {
                foreach (var hata in sonuc.Errors)
                {
                    var hataMesaji =
                        hata.Code == "PasswordMismatch"
                            ? "Mevcut parola hatalıdır."
                            : hata.Description;

                    ModelState.AddModelError(
                        string.Empty,
                        hataMesaji);
                }

                return View(model);
            }

            /*
             * Parola değiştikten sonra mevcut oturumun
             * geçerli kalması sağlanır.
             */
            await _signInManager
                .RefreshSignInAsync(user);

            await _auditService.LogAsync(
                "Şifre Değişikliği",
                "Kullanıcı hesabının parolası değiştirildi.",
                "Başarılı",
                user.Email ?? user.UserName ?? "Kullanıcı");

            TempData["BasariliMesaj"] =
                "Parolanız başarıyla değiştirildi.";

            return RedirectToAction(
                nameof(Profil));
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
    LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            var kullanici =
                await _userManager.FindByEmailAsync(email);

            if (kullanici is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-posta adresi veya parola hatalıdır.");

                return View(model);
            }

            /*
             * Kullanıcı pasif olsa bile giriş yapabilir.
             * Pasiflik kontrolü ödeme ve tahsilat gibi
             * işlem yapan endpointlerde gerçekleştirilecektir.
             */
            var sonuc =
                await _signInManager.PasswordSignInAsync(
                    kullanici,
                    model.Password,
                    model.BeniHatirla,
                    lockoutOnFailure: true);

            if (sonuc.Succeeded)
            {
                var musteriAktifMi = true;

                if (kullanici.MusteriId.HasValue)
                {
                    musteriAktifMi =
                        await _context.Musteriler
                            .AsNoTracking()
                            .Where(x =>
                                x.Id == kullanici.MusteriId.Value)
                            .Select(x => x.AktifMi)
                            .FirstOrDefaultAsync();
                }

                /*
                 * Kullanıcı hesabı veya bağlı müşteri kaydı
                 * pasifse giriş yapılır fakat uyarı gösterilir.
                 */
                if (!kullanici.AktifMi ||
                    !musteriAktifMi)
                {
                    TempData["UyariMesaji"] =
                        "Hesabınız pasif durumdadır. " +
                        "Borçlarınızı ve tahsilat geçmişinizi " +
                        "görüntüleyebilirsiniz ancak ödeme işlemi " +
                        "gerçekleştiremezsiniz.";
                }

                try
                {
                    await _vadeBildirimService
                        .KontrolEtAsync();
                }
                catch (Exception)
                {
                    /*
                     * Bildirim kontrolündeki bir hata,
                     * kullanıcının giriş yapmasını engellemez.
                     */
                }

                if (!string.IsNullOrWhiteSpace(
                        model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(
                        model.ReturnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            if (sonuc.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Çok sayıda hatalı giriş yapıldı. " +
                    "Hesap geçici olarak kilitlendi.");

                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "E-posta adresi veya parola hatalıdır.");

            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
    RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var musteriNo =
                model.MusteriNo.Trim();

            var tckn =
                model.Tckn.Trim();

            var email =
                model.Email.Trim().ToLowerInvariant();

            /*
             * Kullanıcı tarafından girilen üç bilgi de
             * aynı müşteri kaydıyla eşleşmelidir.
             */
            var musteri = await _context.Musteriler
                .FirstOrDefaultAsync(x =>
                    x.MusteriNo == musteriNo &&
                    x.Tckn == tckn &&
                    x.Email != null &&
                    x.Email.ToLower() == email);

            if (musteri is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Girilen müşteri numarası, T.C. kimlik numarası " +
                    "ve e-posta bilgileri sistem kayıtlarıyla eşleşmedi.");

                return View(model);
            }

            if (!musteri.AktifMi)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bu müşteri kaydı aktif değildir.");

                return View(model);
            }

            /*
             * Bu müşteri daha önce bir Identity hesabına
             * bağlanmış mı kontrol edilir.
             */
            var mevcutMusteriHesabi =
                await _userManager.Users
                    .AnyAsync(x =>
                        x.MusteriId == musteri.Id);

            if (mevcutMusteriHesabi)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bu müşteriye ait bir kullanıcı hesabı zaten bulunmaktadır.");

                return View(model);
            }

            /*
             * Aynı e-posta ile başka Identity hesabı
             * açılmasını engeller.
             */
            var mevcutEmailHesabi =
                await _userManager.FindByEmailAsync(email);

            if (mevcutEmailHesabi is not null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Bu e-posta adresiyle daha önce hesap oluşturulmuştur.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,

                AdSoyad = musteri.AdSoyad,

                MusteriId = musteri.Id,

                AktifMi = true,

                OlusturmaTarihi = DateTime.Now
            };

            var sonuc =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!sonuc.Succeeded)
            {
                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        hata.Description);
                }

                return View(model);
            }

            /*
             * Kendi hesabını oluşturan kişi her zaman
             * normal Kullanıcı rolünde oluşturulur.
             *
             * Kayıt ekranından Admin olunamaz.
             */
            var rolSonucu =
                await _userManager.AddToRoleAsync(
                    user,
                    "Kullanici");

            if (!rolSonucu.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "Kullanıcı rolü atanamadığı için hesap oluşturulamadı.");

                return View(model);
            }

            await _auditService.LogAsync(
                "Kullanıcı Kaydı",
                 $"{musteri.MusteriNo} numaralı müşteri için " +
                "kullanıcı hesabı oluşturuldu.",
                "Başarılı",
                user.Email ?? user.UserName ?? email);

            /*
             * Hesap oluşturulduktan sonra kullanıcı
             * doğrudan oturum açmış olur.
             */
            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            TempData["BasariliMesaj"] =
                "Hesabınız başarıyla oluşturuldu.";

            return RedirectToAction(
                "Borclarim",
                "Borclar");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                nameof(Login), "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied(
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
    }
}
