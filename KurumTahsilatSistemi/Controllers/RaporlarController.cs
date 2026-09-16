using ClosedXML.Excel;
using KurumTahsilatSistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RaporlarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RaporlarController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Raporlar
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Raporlar/TahsilatlarExcel
        [HttpGet]
        public async Task<IActionResult> TahsilatlarExcel(
            DateTime? baslangicTarihi,
            DateTime? bitisTarihi)
        {
            var sorgu = _context.Tahsilatlar
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .Include(x => x.Borc)
                    .ThenInclude(x => x.TahsilatUrunu)
                        .ThenInclude(x => x.Kurum)
                .AsQueryable();

            if (baslangicTarihi.HasValue)
            {
                sorgu = sorgu.Where(x =>
                    x.IslemTarihi >=
                    baslangicTarihi.Value.Date);
            }

            if (bitisTarihi.HasValue)
            {
                var sonrakiGun =
                    bitisTarihi.Value.Date.AddDays(1);

                sorgu = sorgu.Where(x =>
                    x.IslemTarihi < sonrakiGun);
            }

            var tahsilatlar = await sorgu
                .OrderByDescending(x => x.IslemTarihi)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sayfa =
                workbook.Worksheets.Add("Tahsilatlar");

            RaporBasliginiOlustur(
                sayfa,
                "TAHSİLAT RAPORU",
                12);

            var basliklar = new[]
            {
                "İşlem No",
                "İşlem Tarihi",
                "Müşteri No",
                "Müşteri Adı",
                "Kurum",
                "Tahsilat Ürünü",
                "Referans No",
                "Ödenen Tutar",
                "Ödeme Kanalı",
                "İşlem Durumu",
                "İşlemi Yapan",
                "İptal Nedeni"
            };

            BaslikSatiriniOlustur(
                sayfa,
                basliklar,
                4);

            var satir = 5;

            foreach (var tahsilat in tahsilatlar)
            {
                sayfa.Cell(satir, 1).Value =
                    tahsilat.IslemNo ?? "-";

                sayfa.Cell(satir, 2).Value =
                    tahsilat.IslemTarihi;

                sayfa.Cell(satir, 3).Value =
                    tahsilat.Borc?.MusteriNo ?? "-";

                sayfa.Cell(satir, 4).Value =
                    tahsilat.Borc?.MusteriAdi ?? "-";

                sayfa.Cell(satir, 5).Value =
                    tahsilat.Borc?
                        .TahsilatUrunu?
                        .Kurum?
                        .KurumAdi ?? "-";

                sayfa.Cell(satir, 6).Value =
                    tahsilat.Borc?
                        .TahsilatUrunu?
                        .UrunAdi ?? "-";

                sayfa.Cell(satir, 7).Value =
                    tahsilat.Borc?.ReferansNo ?? "-";

                sayfa.Cell(satir, 8).Value =
                    tahsilat.OdenenTutar;

                sayfa.Cell(satir, 9).Value =
                    tahsilat.OdemeKanali ?? "-";

                sayfa.Cell(satir, 10).Value =
                    tahsilat.IslemDurumu ?? "-";

                sayfa.Cell(satir, 11).Value =
                    tahsilat.ApplicationUser?.Email
                    ?? "-";

                sayfa.Cell(satir, 12).Value =
                    tahsilat.IptalNedeni ?? "-";

                satir++;
            }

            ExcelSayfasiniBicimlendir(
                sayfa,
                baslikSatiri: 4,
                sonSutun: 12,
                sonSatir: satir - 1);

            sayfa.Column(2)
                .Style.DateFormat.Format =
                "dd.MM.yyyy HH:mm";

            sayfa.Column(8)
                .Style.NumberFormat.Format =
                "#,##0.00 \"TL\"";

            sayfa.SheetView.FreezeRows(4);

            return ExcelDosyasiOlustur(
                workbook,
                $"Tahsilat-Raporu-" +
                $"{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        // GET: Raporlar/BorclarExcel
        [HttpGet]
        public async Task<IActionResult> BorclarExcel()
        {
            var borclar = await _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .OrderBy(x => x.SonOdemeTarihi)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sayfa =
                workbook.Worksheets.Add("Borçlar");

            RaporBasliginiOlustur(
                sayfa,
                "BORÇ RAPORU",
                13);

            var basliklar = new[]
            {
                "Referans No",
                "Müşteri No",
                "Müşteri Adı",
                "Kurum",
                "Tahsilat Ürünü",
                "Toplam Borç",
                "Ödenen Tutar",
                "Kalan Tutar",
                "Son Ödeme Tarihi",
                "Durum",
                "Gecikmiş Mi",
                "Geciken Gün",
                "Oluşturulma Tarihi"
            };

            BaslikSatiriniOlustur(
                sayfa,
                basliklar,
                4);

            var bugun = DateTime.Today;
            var satir = 5;

            foreach (var borc in borclar)
            {
                var kalanTutar = Math.Max(
                    0,
                    borc.BorcTutari -
                    borc.OdenenTutar);

                var gecikmisMi =
                    borc.Durum != "Ödendi" &&
                    borc.SonOdemeTarihi.Date <
                    bugun;

                var gecikenGun =
                    gecikmisMi
                        ? (bugun -
                           borc.SonOdemeTarihi.Date).Days
                        : 0;

                sayfa.Cell(satir, 1).Value =
                    borc.ReferansNo;

                sayfa.Cell(satir, 2).Value =
                    borc.MusteriNo;

                sayfa.Cell(satir, 3).Value =
                    borc.MusteriAdi;

                sayfa.Cell(satir, 4).Value =
                    borc.TahsilatUrunu?
                        .Kurum?
                        .KurumAdi ?? "-";

                sayfa.Cell(satir, 5).Value =
                    borc.TahsilatUrunu?
                        .UrunAdi ?? "-";

                sayfa.Cell(satir, 6).Value =
                    borc.BorcTutari;

                sayfa.Cell(satir, 7).Value =
                    borc.OdenenTutar;

                sayfa.Cell(satir, 8).Value =
                    kalanTutar;

                sayfa.Cell(satir, 9).Value =
                    borc.SonOdemeTarihi;

                sayfa.Cell(satir, 10).Value =
                    borc.Durum;

                sayfa.Cell(satir, 11).Value =
                    gecikmisMi
                        ? "Evet"
                        : "Hayır";

                sayfa.Cell(satir, 12).Value =
                    gecikenGun;

                sayfa.Cell(satir, 13).Value =
                    borc.OlusturmaTarihi;

                satir++;
            }

            ExcelSayfasiniBicimlendir(
                sayfa,
                baslikSatiri: 4,
                sonSutun: 13,
                sonSatir: satir - 1);

            sayfa.Columns(6, 8)
                .Style.NumberFormat.Format =
                "#,##0.00 \"TL\"";

            sayfa.Column(9)
                .Style.DateFormat.Format =
                "dd.MM.yyyy";

            sayfa.Column(13)
                .Style.DateFormat.Format =
                "dd.MM.yyyy HH:mm";

            sayfa.SheetView.FreezeRows(4);

            return ExcelDosyasiOlustur(
                workbook,
                $"Borc-Raporu-" +
                $"{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        // GET: Raporlar/MusterilerExcel
        [HttpGet]
        public async Task<IActionResult> MusterilerExcel()
        {
            var musteriler = await _context.Musteriler
                .AsNoTracking()
                .OrderBy(x => x.AdSoyad)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sayfa =
                workbook.Worksheets.Add("Müşteriler");

            RaporBasliginiOlustur(
                sayfa,
                "MÜŞTERİ RAPORU",
                10);

            var basliklar = new[]
            {
                "Müşteri No",
                "Ad Soyad / Unvan",
                "T.C. Kimlik No",
                "Vergi No",
                "E-posta",
                "Telefon",
                "Adres",
                "Aktif Mi",
                "Oluşturulma Tarihi",
                "Müşteri Türü"
            };

            BaslikSatiriniOlustur(
                sayfa,
                basliklar,
                4);

            var satir = 5;

            foreach (var musteri in musteriler)
            {
                sayfa.Cell(satir, 1).Value =
                    musteri.MusteriNo;

                sayfa.Cell(satir, 2).Value =
                    musteri.AdSoyad;

                sayfa.Cell(satir, 3).Value =
                    musteri.Tckn ?? "-";

                sayfa.Cell(satir, 4).Value =
                    musteri.VergiNo ?? "-";

                sayfa.Cell(satir, 5).Value =
                    musteri.Email ?? "-";

                sayfa.Cell(satir, 6).Value =
                    musteri.Telefon ?? "-";

                sayfa.Cell(satir, 7).Value =
                    musteri.Adres ?? "-";

                sayfa.Cell(satir, 8).Value =
                    musteri.AktifMi
                        ? "Aktif"
                        : "Pasif";

                sayfa.Cell(satir, 9).Value =
                    musteri.OlusturmaTarihi;

                sayfa.Cell(satir, 10).Value =
                    !string.IsNullOrWhiteSpace(
                        musteri.VergiNo)
                        ? "Kurumsal"
                        : "Bireysel";

                satir++;
            }

            ExcelSayfasiniBicimlendir(
                sayfa,
                baslikSatiri: 4,
                sonSutun: 10,
                sonSatir: satir - 1);

            sayfa.Column(9)
                .Style.DateFormat.Format =
                "dd.MM.yyyy HH:mm";

            sayfa.SheetView.FreezeRows(4);

            return ExcelDosyasiOlustur(
                workbook,
                $"Musteri-Raporu-" +
                $"{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        // GET: Raporlar/AuditLogExcel
        [HttpGet]
        public async Task<IActionResult> AuditLogExcel()
        {
            var loglar = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.IslemTarihi)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sayfa =
                workbook.Worksheets.Add("İşlem Logları");

            RaporBasliginiOlustur(
                sayfa,
                "İŞLEM LOGU RAPORU",
                8);

            var basliklar = new[]
            {
                "İşlem Tarihi",
                "Kullanıcı",
                "İşlem Tipi",
                "Açıklama",
                "Durum",
                "IP Adresi",
                "Controller",
                "Action"
            };

            BaslikSatiriniOlustur(
                sayfa,
                basliklar,
                4);

            var satir = 5;

            foreach (var log in loglar)
            {
                sayfa.Cell(satir, 1).Value =
                    log.IslemTarihi;

                sayfa.Cell(satir, 2).Value =
                    log.Kullanici ?? "-";

                sayfa.Cell(satir, 3).Value =
                    log.IslemTipi ?? "-";

                sayfa.Cell(satir, 4).Value =
                    log.Aciklama ?? "-";

                sayfa.Cell(satir, 5).Value =
                    log.Durum ?? "-";

                sayfa.Cell(satir, 6).Value =
                    log.IPAdresi ?? "-";

                sayfa.Cell(satir, 7).Value =
                    log.Controller ?? "-";

                sayfa.Cell(satir, 8).Value =
                    log.Action ?? "-";

                satir++;
            }

            ExcelSayfasiniBicimlendir(
                sayfa,
                baslikSatiri: 4,
                sonSutun: 8,
                sonSatir: satir - 1);

            sayfa.Column(1)
                .Style.DateFormat.Format =
                "dd.MM.yyyy HH:mm:ss";

            sayfa.Column(4).Width = 55;
            sayfa.Column(4).Style.Alignment.WrapText =
                true;

            sayfa.SheetView.FreezeRows(4);

            return ExcelDosyasiOlustur(
                workbook,
                $"Islem-Logu-Raporu-" +
                $"{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        private static void RaporBasliginiOlustur(
            IXLWorksheet sayfa,
            string raporBasligi,
            int sonSutun)
        {
            sayfa.Cell(1, 1).Value =
                raporBasligi;

            var baslikAraligi =
                sayfa.Range(
                    1,
                    1,
                    1,
                    sonSutun);

            baslikAraligi.Merge();

            baslikAraligi.Style.Font.Bold =
                true;

            baslikAraligi.Style.Font.FontSize =
                18;

            baslikAraligi.Style.Font.FontColor =
                XLColor.White;

            baslikAraligi.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#123B6D");

            baslikAraligi.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            baslikAraligi.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            sayfa.Row(1).Height = 32;

            sayfa.Cell(2, 1).Value =
                $"Rapor oluşturma tarihi: " +
                $"{DateTime.Now:dd.MM.yyyy HH:mm}";

            var tarihAraligi =
                sayfa.Range(
                    2,
                    1,
                    2,
                    sonSutun);

            tarihAraligi.Merge();

            tarihAraligi.Style.Font.Italic =
                true;

            tarihAraligi.Style.Font.FontColor =
                XLColor.FromHtml("#667085");

            tarihAraligi.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Right;
        }

        private static void BaslikSatiriniOlustur(
            IXLWorksheet sayfa,
            IReadOnlyList<string> basliklar,
            int baslikSatiri)
        {
            for (var sutun = 0;
                 sutun < basliklar.Count;
                 sutun++)
            {
                sayfa.Cell(
                    baslikSatiri,
                    sutun + 1).Value =
                    basliklar[sutun];
            }
        }

        private static void ExcelSayfasiniBicimlendir(
            IXLWorksheet sayfa,
            int baslikSatiri,
            int sonSutun,
            int sonSatir)
        {
            var baslikAraligi =
                sayfa.Range(
                    baslikSatiri,
                    1,
                    baslikSatiri,
                    sonSutun);

            baslikAraligi.Style.Font.Bold =
                true;

            baslikAraligi.Style.Font.FontColor =
                XLColor.White;

            baslikAraligi.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1D5B9E");

            baslikAraligi.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            baslikAraligi.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            sayfa.Row(baslikSatiri).Height =
                25;

            if (sonSatir >= baslikSatiri + 1)
            {
                var veriAraligi =
                    sayfa.Range(
                        baslikSatiri,
                        1,
                        sonSatir,
                        sonSutun);

                veriAraligi.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                veriAraligi.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;

                veriAraligi.Style.Border.OutsideBorderColor =
                    XLColor.FromHtml("#DCE4EE");

                veriAraligi.Style.Border.InsideBorderColor =
                    XLColor.FromHtml("#DCE4EE");

                sayfa.Range(
                        baslikSatiri,
                        1,
                        sonSatir,
                        sonSutun)
                    .CreateTable();
            }

            sayfa.Columns(
                    1,
                    sonSutun)
                .AdjustToContents();

            foreach (var sutun in
                     sayfa.Columns(1, sonSutun))
            {
                if (sutun.Width > 45)
                {
                    sutun.Width = 45;
                }

                if (sutun.Width < 12)
                {
                    sutun.Width = 12;
                }
            }

            sayfa.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }

        private FileContentResult ExcelDosyasiOlustur(
            XLWorkbook workbook,
            string dosyaAdi)
        {
            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-" +
                "officedocument.spreadsheetml.sheet",
                dosyaAdi);
        }
    }
}