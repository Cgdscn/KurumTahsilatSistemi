using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Services
{
    public class VadeBildirimService : IVadeBildirimService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VadeBildirimService> _logger;

        public VadeBildirimService(
            ApplicationDbContext context,
            ILogger<VadeBildirimService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task KontrolEtAsync(
            CancellationToken cancellationToken = default)
        {
            var bugun = DateTime.Today;

            /*
             * Yalnızca tamamen ödenmemiş borçlar alınır.
             *
             * Tarihi üç günden daha ileri olan borçların
             * şu anda bildirim üretmesine gerek yoktur.
             */
            var borclar = await _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .Where(x =>
                    x.Durum != "Ödendi" &&
                    x.MusteriId != null &&
                    x.SonOdemeTarihi.Date <=
                        bugun.AddDays(3))
                .ToListAsync(cancellationToken);

            if (borclar.Count == 0)
            {
                _logger.LogInformation(
                    "Vade kontrolünde uygun borç bulunamadı.");

                return;
            }

            /*
             * Borçların bağlı olduğu müşterilere ait
             * Identity kullanıcı hesapları alınır.
             */
            var musteriIdleri = borclar
                .Where(x => x.MusteriId.HasValue)
                .Select(x => x.MusteriId!.Value)
                .Distinct()
                .ToList();

            var kullanicilar = await _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.AktifMi &&
                    x.MusteriId != null &&
                    musteriIdleri.Contains(
                        x.MusteriId.Value))
                .Select(x => new
                {
                    x.Id,
                    x.MusteriId
                })
                .ToListAsync(cancellationToken);

            var olusturulanBildirimSayisi = 0;

            foreach (var borc in borclar)
            {
                if (!borc.MusteriId.HasValue)
                {
                    continue;
                }

                var kullanici = kullanicilar
                    .FirstOrDefault(x =>
                        x.MusteriId ==
                        borc.MusteriId.Value);

                /*
                 * Müşteri henüz kullanıcı hesabı
                 * oluşturmadıysa bildirim yazılmaz.
                 */
                if (kullanici is null)
                {
                    continue;
                }

                var kalanTutar = Math.Max(
                    0,
                    borc.BorcTutari -
                    borc.OdenenTutar);

                if (kalanTutar <= 0)
                {
                    continue;
                }

                var kalanGun =
                    (borc.SonOdemeTarihi.Date -
                     bugun).Days;

                string bildirimKodu;
                string baslik;
                string mesaj;
                string bildirimTipi;

                if (kalanGun == 3)
                {
                    bildirimKodu =
                        $"BORC-{borc.Id}-VADE-3-GUN";

                    baslik =
                        "Son Ödeme Tarihi Yaklaşıyor";

                    mesaj =
                        $"{borc.TahsilatUrunu?.Kurum?.KurumAdi ?? "Kurum"} " +
                        $"{borc.TahsilatUrunu?.UrunAdi ?? "borç"} kaydınıza ait " +
                        $"{kalanTutar:N2} TL kalan borcun son ödeme " +
                        $"tarihine 3 gün kaldı. Son ödeme tarihi: " +
                        $"{borc.SonOdemeTarihi:dd.MM.yyyy}.";

                    bildirimTipi = "Uyarı";
                }
                else if (kalanGun == 0)
                {
                    bildirimKodu =
                        $"BORC-{borc.Id}-VADE-BUGUN";

                    baslik =
                        "Son Ödeme Tarihi Bugün";

                    mesaj =
                        $"{borc.ReferansNo} referans numaralı " +
                        $"{kalanTutar:N2} TL kalan borcunuzun " +
                        "son ödeme tarihi bugündür.";

                    bildirimTipi = "Uyarı";
                }
                else if (kalanGun < 0)
                {
                    var gecikenGunSayisi =
                        Math.Abs(kalanGun);

                    bildirimKodu =
                        $"BORC-{borc.Id}-GECIKMIS";

                    baslik =
                        "Borcunuzun Vadesi Geçti";

                    mesaj =
                        $"{borc.ReferansNo} referans numaralı " +
                        $"{kalanTutar:N2} TL kalan borcunuz " +
                        $"{gecikenGunSayisi} gündür gecikmiş durumdadır.";

                    bildirimTipi = "Hata";
                }
                else
                {
                    /*
                     * Örneğin 1 veya 2 gün kalan borçlar için
                     * bu aşamada ayrı bildirim oluşturulmaz.
                     */
                    continue;
                }

                var bildirimVarMi =
                    await _context.Bildirimler
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.ApplicationUserId ==
                                kullanici.Id &&
                            x.BildirimKodu ==
                                bildirimKodu,
                            cancellationToken);

                if (bildirimVarMi)
                {
                    continue;
                }

                var bildirim = new Bildirim
                {
                    ApplicationUserId =
                        kullanici.Id,

                    BorcId =
                        borc.Id,

                    BildirimKodu =
                        bildirimKodu,

                    Baslik =
                        baslik,

                    Mesaj =
                        mesaj,

                    BildirimTipi =
                        bildirimTipi,

                    OkunduMu =
                        false,

                    OlusturmaTarihi =
                        DateTime.Now,

                    YonlendirmeUrl =
                        "/Borclar/Borclarim"
                };

                _context.Bildirimler.Add(
                    bildirim);

                olusturulanBildirimSayisi++;
            }

            if (olusturulanBildirimSayisi > 0)
            {
                try
                {
                    await _context.SaveChangesAsync(
                        cancellationToken);
                }
                catch (DbUpdateException hata)
                {
                    /*
                     * Birden fazla uygulama örneği aynı anda
                     * çalışıyorsa benzersiz indeks tekrar
                     * oluşturmayı engelleyebilir.
                     */
                    _logger.LogWarning(
                        hata,
                        "Bazı vade bildirimleri daha önce oluşturulmuş olabilir.");
                }
            }

            _logger.LogInformation(
                "Vade kontrolü tamamlandı. {BildirimSayisi} bildirim oluşturuldu.",
                olusturulanBildirimSayisi);
        }
    }
}