using KurumTahsilatSistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Controllers.Api
{
    [ApiController]
    [Route("api/borclar")]
    [AllowAnonymous]
    public class BorclarApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BorclarApiController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /*
         * GET:
         * /api/borclar/aktif
         *
         * Filtreli örnek:
         * /api/borclar/aktif?musteriNo=MUS1002
         */
        [HttpGet("aktif")]
        public async Task<IActionResult> AktifBorclar(
            [FromQuery] string? musteriNo,
            [FromQuery] string? referansNo,
            CancellationToken cancellationToken)
        {
            /*
             * Bu endpoint yalnızca geliştirme ortamında çalışır.
             * Proje canlı ortama geçtiğinde borç bilgileri
             * anonim olarak erişilebilir olmaz.
             */
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var bugun = DateTime.Today;

            var sorgu = _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .Where(x =>
                    x.Durum != "Ödendi" &&
                    x.BorcTutari > x.OdenenTutar)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(musteriNo))
            {
                var temizMusteriNo =
                    musteriNo.Trim();

                sorgu = sorgu.Where(x =>
                    x.Musteri != null &&
                    x.Musteri.MusteriNo.Contains(
                        temizMusteriNo));
            }

            if (!string.IsNullOrWhiteSpace(referansNo))
            {
                var temizReferansNo =
                    referansNo.Trim();

                sorgu = sorgu.Where(x =>
                    x.ReferansNo.Contains(
                        temizReferansNo));
            }

            var borclar = await sorgu
                .OrderBy(x => x.SonOdemeTarihi)
                .Select(x => new
                {
                    id =
                        x.Id,

                    musteriNo =
                        x.Musteri != null
                            ? x.Musteri.MusteriNo
                            : null,

                    musteriAdi =
                        x.Musteri != null
                            ? x.Musteri.AdSoyad
                            : x.MusteriAdi,

                    kurum =
                        x.TahsilatUrunu.Kurum.KurumAdi,

                    tahsilatUrunu =
                        x.TahsilatUrunu.UrunAdi,

                    referansNo =
                        x.ReferansNo,

                    borcTutari =
                        x.BorcTutari,

                    odenenTutar =
                        x.OdenenTutar,

                    kalanTutar =
                        x.BorcTutari -
                        x.OdenenTutar,

                    sonOdemeTarihi =
                        x.SonOdemeTarihi,

                    durum =
                        x.Durum,

                    gecikmisMi =
                        x.SonOdemeTarihi < bugun
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                basarili = true,
                kayitSayisi = borclar.Count,
                borclar
            });
        }

        /*
         * GET:
         * /api/borclar/10
         */
        [HttpGet("{id:int}")]
        public async Task<IActionResult> BorcDetayi(
            int id,
            CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var borc = await _context.Borclar
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id =
                        x.Id,

                    musteriNo =
                        x.Musteri != null
                            ? x.Musteri.MusteriNo
                            : null,

                    musteriAdi =
                        x.Musteri != null
                            ? x.Musteri.AdSoyad
                            : x.MusteriAdi,

                    kurum =
                        x.TahsilatUrunu.Kurum.KurumAdi,

                    tahsilatUrunu =
                        x.TahsilatUrunu.UrunAdi,

                    referansNo =
                        x.ReferansNo,

                    borcTutari =
                        x.BorcTutari,

                    odenenTutar =
                        x.OdenenTutar,

                    kalanTutar =
                        x.BorcTutari -
                        x.OdenenTutar,

                    sonOdemeTarihi =
                        x.SonOdemeTarihi,

                    durum =
                        x.Durum
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (borc is null)
            {
                return NotFound(new
                {
                    basarili = false,
                    mesaj = "Borç kaydı bulunamadı."
                });
            }

            return Ok(new
            {
                basarili = true,
                borc
            });
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok(new
            { basarili = true,
              mesaj = "Kurum Tahsilat API çalışıyor"
            });
        }
    }
}