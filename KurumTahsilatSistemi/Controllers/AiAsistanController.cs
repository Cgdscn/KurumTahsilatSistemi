using System.Security.Claims;
using System.Text.Json;
using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin,Kullanici")]
    public class AiAsistanController : Controller
    {
        private readonly IAiChatService _aiChatService;
        private readonly ApplicationDbContext _context;

        public AiAsistanController(
            IAiChatService aiChatService,
            ApplicationDbContext context)
        {
            _aiChatService = aiChatService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new AiChatViewModel
            {
                Mesajlar = SohbetiGetir()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            AiChatViewModel model,
            CancellationToken cancellationToken)
        {
            var mesajlar = SohbetiGetir();

            if (!ModelState.IsValid)
            {
                model.Mesajlar = mesajlar;

                return View(model);
            }

            var kullaniciMesaji =
                model.Mesaj.Trim();

            mesajlar.Add(new AiChatMessageViewModel
            {
                Rol = "user",
                Icerik = kullaniciMesaji,
                Zaman = DateTime.Now
            });

            var kullaniciBaglami =
                await KullaniciBaglaminiGetirAsync(
                    cancellationToken);

            if (kullaniciBaglami is null)
            {
                mesajlar.Add(new AiChatMessageViewModel
                {
                    Rol = "assistant",
                    Icerik =
                        "Hesabınıza bağlı geçerli bir kullanıcı " +
                        "veya müşteri kaydı bulunamadı.",
                    Zaman = DateTime.Now
                });

                SohbetiKaydet(mesajlar);

                return RedirectToAction(
                    nameof(Index));
            }

            try
            {
                var cevap =
                    await _aiChatService.CevaplaAsync(
                        mesajlar,
                        kullaniciBaglami,
                        cancellationToken);

                mesajlar.Add(new AiChatMessageViewModel
                {
                    Rol = "assistant",
                    Icerik = cevap,
                    Zaman = DateTime.Now
                });
            }
            catch (HttpRequestException)
            {
                mesajlar.Add(new AiChatMessageViewModel
                {
                    Rol = "assistant",
                    Icerik =
                        "Yerel AI servisine ulaşılamadı. " +
                        "Ollama uygulamasının çalıştığından " +
                        "emin olunuz.",
                    Zaman = DateTime.Now
                });
            }
            catch (TaskCanceledException)
            {
                mesajlar.Add(new AiChatMessageViewModel
                {
                    Rol = "assistant",
                    Icerik =
                        "AI yanıtı zaman aşımına uğradı. " +
                        "Lütfen tekrar deneyiniz.",
                    Zaman = DateTime.Now
                });
            }
            catch (Exception)
            {
                mesajlar.Add(new AiChatMessageViewModel
                {
                    Rol = "assistant",
                    Icerik =
                        "AI Asistan şu anda yanıt oluşturamıyor.",
                    Zaman = DateTime.Now
                });
            }

            SohbetiKaydet(mesajlar);

            return RedirectToAction(
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Temizle()
        {
            HttpContext.Session.Remove(
                SohbetSessionAnahtari());

            return RedirectToAction(
                nameof(Index));
        }

        [HttpGet]
        public IActionResult PopupGecmis()
        {
            var mesajlar = SohbetiGetir()
                .Select(x => new
                {
                    rol = x.Rol,
                    icerik = x.Icerik,
                    zaman = x.Zaman.ToString("HH:mm")
                })
                .ToList();

            return Json(mesajlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PopupMesaj(
            string mesaj,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(mesaj))
            {
                return BadRequest(new
                {
                    hata = "Lütfen bir mesaj yazınız."
                });
            }

            mesaj = mesaj.Trim();

            if (mesaj.Length > 500)
            {
                return BadRequest(new
                {
                    hata =
                        "Mesaj en fazla 500 karakter olabilir."
                });
            }

            var kullaniciBaglami =
                await KullaniciBaglaminiGetirAsync(
                    cancellationToken);

            if (kullaniciBaglami is null)
            {
                return BadRequest(new
                {
                    hata =
                        "Hesabınıza bağlı geçerli bir kullanıcı " +
                        "veya müşteri kaydı bulunamadı."
                });
            }

            var mesajlar = SohbetiGetir();

            mesajlar.Add(new AiChatMessageViewModel
            {
                Rol = "user",
                Icerik = mesaj,
                Zaman = DateTime.Now
            });

            string cevap;

            try
            {
                cevap =
                    await _aiChatService.CevaplaAsync(
                        mesajlar,
                        kullaniciBaglami,
                        cancellationToken);
            }
            catch (HttpRequestException)
            {
                cevap =
                    "Yerel AI servisine ulaşılamadı. " +
                    "Ollama uygulamasının çalıştığından " +
                    "emin olunuz.";
            }
            catch (TaskCanceledException)
            {
                cevap =
                    "AI yanıtı zaman aşımına uğradı. " +
                    "Lütfen tekrar deneyiniz.";
            }
            catch (Exception)
            {
                cevap =
                    "AI Asistan şu anda yanıt oluşturamıyor.";
            }

            var cevapZamani =
                DateTime.Now;

            mesajlar.Add(new AiChatMessageViewModel
            {
                Rol = "assistant",
                Icerik = cevap,
                Zaman = cevapZamani
            });

            SohbetiKaydet(mesajlar);

            return Json(new
            {
                cevap,
                zaman =
                    cevapZamani.ToString("HH:mm")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PopupTemizle()
        {
            HttpContext.Session.Remove(
                SohbetSessionAnahtari());

            return Json(new
            {
                basarili = true
            });
        }

        private async Task<AiKullaniciBaglami?>
            KullaniciBaglaminiGetirAsync(
                CancellationToken cancellationToken)
        {
            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(
                    kullaniciId))
            {
                return null;
            }

            var kullanici =
                await _context.Users
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == kullaniciId)
                    .Select(x => new
                    {
                        x.Id,
                        x.MusteriId,
                        x.AktifMi
                    })
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (kullanici is null)
            {
                return null;
            }

            var adminMi =
                User.IsInRole("Admin");

            /*
             * Normal kullanıcının mutlaka kendisine
             * bağlı bir müşteri kaydı olmalıdır.
             */
            if (!adminMi &&
                !kullanici.MusteriId.HasValue)
            {
                return null;
            }

            var musteriAktifMi = true;

            if (kullanici.MusteriId.HasValue)
            {
                musteriAktifMi =
                    await _context.Musteriler
                        .AsNoTracking()
                        .Where(x =>
                            x.Id ==
                            kullanici.MusteriId.Value)
                        .Select(x =>
                            x.AktifMi)
                        .FirstOrDefaultAsync(
                            cancellationToken);
            }

            return new AiKullaniciBaglami
            {
                KullaniciId =
                    kullanici.Id,

                MusteriId =
                    kullanici.MusteriId,

                AdminMi =
                    adminMi,

                KullaniciAktifMi =
                    kullanici.AktifMi,

                MusteriAktifMi =
                    musteriAktifMi
            };
        }

        private List<AiChatMessageViewModel>
            SohbetiGetir()
        {
            var json =
                HttpContext.Session.GetString(
                    SohbetSessionAnahtari());

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<
                    AiChatMessageViewModel>();
            }

            try
            {
                return JsonSerializer.Deserialize<
                           List<AiChatMessageViewModel>>(
                           json)
                       ?? new List<
                           AiChatMessageViewModel>();
            }
            catch (JsonException)
            {
                HttpContext.Session.Remove(
                    SohbetSessionAnahtari());

                return new List<
                    AiChatMessageViewModel>();
            }
        }

        private void SohbetiKaydet(
            List<AiChatMessageViewModel> mesajlar)
        {
            var saklanacakMesajlar =
                mesajlar
                    .TakeLast(40)
                    .ToList();

            var json =
                JsonSerializer.Serialize(
                    saklanacakMesajlar);

            HttpContext.Session.SetString(
                SohbetSessionAnahtari(),
                json);
        }

        private string SohbetSessionAnahtari()
        {
            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? "BilinmeyenKullanici";

            return $"AiSohbet:{kullaniciId}";
        }
    }
}