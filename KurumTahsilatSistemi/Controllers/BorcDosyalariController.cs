using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using KurumTahsilatSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorcDosyalariController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBildirimService _bildirimService;

        public BorcDosyalariController(
            ApplicationDbContext context,
            IBildirimService bildirimService)
        {
            _context = context;
            _bildirimService = bildirimService;
        }

        [HttpGet]
        public IActionResult Yukle()
        {
            ListeleriDoldur();

            return View(new BorcDosyaYukleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Yukle(
            BorcDosyaYukleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ListeleriDoldur(
                    model.KurumId,
                    model.TahsilatUrunuId);

                return View(model);
            }

            var tahsilatUrunu =
                await _context.TahsilatUrunleri
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.TahsilatUrunuId &&
                        x.KurumId == model.KurumId);

            if (tahsilatUrunu is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Seçilen tahsilat ürünü seçilen kuruma ait değildir.");

                ListeleriDoldur(
                    model.KurumId,
                    model.TahsilatUrunuId);

                return View(model);
            }

            if (model.Dosya is null ||
                model.Dosya.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Dosya),
                    "Geçerli bir dosya seçiniz.");

                ListeleriDoldur(
                    model.KurumId,
                    model.TahsilatUrunuId);

                return View(model);
            }

            var uzanti =
                Path.GetExtension(model.Dosya.FileName);

            if (!string.Equals(
                    uzanti,
                    ".txt",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Dosya),
                    "Yalnızca TXT dosyası yüklenebilir.");

                ListeleriDoldur(
                    model.KurumId,
                    model.TahsilatUrunuId);

                return View(model);
            }

            var aktarim = new DosyaAktarim
            {
                KurumId = model.KurumId,
                DosyaAdi =
                    Path.GetFileName(model.Dosya.FileName),
                YuklemeTarihi = DateTime.Now,
                Durum = "İşleniyor"
            };

            _context.DosyaAktarimlari.Add(aktarim);

            var toplamKayit = 0;
            var basariliKayit = 0;
            var hataliKayit = 0;

            /*
             * Yalnızca yeni oluşturulan borçlar için
             * bildirim hazırlanacaktır.
             */
            var bildirimOlusturulacakBorclar =
                new List<YeniBorcBildirimBilgisi>();

            using var reader =
                new StreamReader(
                    model.Dosya.OpenReadStream());

            while (!reader.EndOfStream)
            {
                var satir =
                    await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(satir))
                {
                    continue;
                }

                toplamKayit++;

                try
                {
                    var alanlar =
                        satir.Split(';');

                    if (alanlar.Length != 5)
                    {
                        hataliKayit++;
                        continue;
                    }

                    var musteriNo =
                        alanlar[0].Trim();

                    var musteriAdi =
                        alanlar[1].Trim();

                    var tutarMetni =
                        alanlar[2].Trim();

                    var tarihMetni =
                        alanlar[3].Trim();

                    var referansNo =
                        alanlar[4].Trim();

                    var tutarBasarili =
                        decimal.TryParse(
                            tutarMetni,
                            NumberStyles.Number,
                            new CultureInfo("tr-TR"),
                            out var borcTutari);

                    var tarihBasarili =
                        DateTime.TryParseExact(
                            tarihMetni,
                            "dd.MM.yyyy",
                            new CultureInfo("tr-TR"),
                            DateTimeStyles.None,
                            out var sonOdemeTarihi);

                    if (!tutarBasarili ||
                        !tarihBasarili ||
                        borcTutari <= 0 ||
                        string.IsNullOrWhiteSpace(musteriNo) ||
                        string.IsNullOrWhiteSpace(musteriAdi) ||
                        string.IsNullOrWhiteSpace(referansNo))
                    {
                        hataliKayit++;
                        continue;
                    }

                    var musteri =
                        await _context.Musteriler
                            .FirstOrDefaultAsync(x =>
                                x.MusteriNo == musteriNo);

                    if (musteri is null)
                    {
                        musteri = new Musteri
                        {
                            MusteriNo = musteriNo,
                            AdSoyad = musteriAdi,
                            AktifMi = true,
                            OlusturmaTarihi = DateTime.Now
                        };

                        _context.Musteriler.Add(musteri);

                        /*
                         * Yeni müşterinin Id değerinin oluşması
                         * için kayıt gerçekleştirilir.
                         */
                        await _context.SaveChangesAsync();
                    }
                    else if (!string.Equals(
                                 musteri.AdSoyad,
                                 musteriAdi,
                                 StringComparison.Ordinal))
                    {
                        musteri.AdSoyad = musteriAdi;
                    }

                    var mevcutBorc =
                        await _context.Borclar
                            .FirstOrDefaultAsync(x =>
                                x.ReferansNo == referansNo);

                    if (mevcutBorc is not null)
                    {
                        if (mevcutBorc.Durum == "Ödendi")
                        {
                            hataliKayit++;
                            continue;
                        }

                        mevcutBorc.MusteriId =
                            musteri.Id;

                        mevcutBorc.MusteriNo =
                            musteri.MusteriNo;

                        mevcutBorc.MusteriAdi =
                            musteri.AdSoyad;

                        mevcutBorc.BorcTutari =
                            borcTutari;

                        mevcutBorc.SonOdemeTarihi =
                            sonOdemeTarihi;

                        mevcutBorc.TahsilatUrunuId =
                            model.TahsilatUrunuId;

                        mevcutBorc.Durum =
                            "Ödenmedi";

                        basariliKayit++;
                        continue;
                    }

                    var yeniBorc = new Borc
                    {
                        MusteriId = musteri.Id,
                        MusteriNo = musteri.MusteriNo,
                        MusteriAdi = musteri.AdSoyad,
                        BorcTutari = borcTutari,
                        SonOdemeTarihi = sonOdemeTarihi,
                        ReferansNo = referansNo,
                        Durum = "Ödenmedi",
                        TahsilatUrunuId =
                            model.TahsilatUrunuId,
                        OlusturmaTarihi = DateTime.Now
                    };

                    _context.Borclar.Add(yeniBorc);

                    bildirimOlusturulacakBorclar.Add(
                        new YeniBorcBildirimBilgisi
                        {
                            MusteriId = musteri.Id,
                            ReferansNo = referansNo,
                            Tutar = borcTutari,
                            SonOdemeTarihi =
                                sonOdemeTarihi
                        });

                    basariliKayit++;
                }
                catch
                {
                    hataliKayit++;
                }
            }

            aktarim.ToplamKayitSayisi =
                toplamKayit;

            aktarim.BasariliKayitSayisi =
                basariliKayit;

            aktarim.HataliKayitSayisi =
                hataliKayit;

            aktarim.Durum =
                hataliKayit == 0
                    ? "Başarılı"
                    : basariliKayit == 0
                        ? "Başarısız"
                        : "Kısmen Başarılı";

            await _context.SaveChangesAsync();

            /*
             * Borçlar veri tabanına kaydedildikten sonra
             * ilgili müşterilerin kullanıcı hesaplarına
             * bildirim gönderilir.
             */
            foreach (var bildirim in bildirimOlusturulacakBorclar)
            {
                await _bildirimService
                    .MusteriyeBildirimOlusturAsync(
                        bildirim.MusteriId,
                        "Yeni Borç Eklendi",
                        $"{bildirim.ReferansNo} referans numaralı " +
                        $"{bildirim.Tutar:N2} TL tutarındaki borç " +
                        $"hesabınıza tanımlandı. Son ödeme tarihi: " +
                        $"{bildirim.SonOdemeTarihi:dd.MM.yyyy}.",
                        "Bilgi",
                        "/Borclar/Borclarim");
            }

            TempData["BasariliMesaj"] =
                $"{basariliKayit} kayıt başarıyla işlendi. " +
                $"{hataliKayit} kayıt hatalı bulundu.";

            return RedirectToAction(nameof(Yukle));
        }

        private void ListeleriDoldur(
            int? kurumId = null,
            int? tahsilatUrunuId = null)
        {
            ViewBag.KurumId = new SelectList(
                _context.Kurumlar
                    .AsNoTracking()
                    .Where(x => x.AktifMi)
                    .OrderBy(x => x.KurumAdi),
                "Id",
                "KurumAdi",
                kurumId);

            ViewBag.TahsilatUrunuId = new SelectList(
                _context.TahsilatUrunleri
                    .AsNoTracking()
                    .Include(x => x.Kurum)
                    .OrderBy(x => x.UrunAdi),
                "Id",
                "UrunAdi",
                tahsilatUrunuId);
        }

        private sealed class YeniBorcBildirimBilgisi
        {
            public int MusteriId { get; set; }

            public string ReferansNo { get; set; }
                = string.Empty;

            public decimal Tutar { get; set; }

            public DateTime SonOdemeTarihi { get; set; }
        }
    }
}