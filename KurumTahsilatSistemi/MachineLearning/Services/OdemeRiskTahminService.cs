using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.MachineLearning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

using MusteriEntity =
    KurumTahsilatSistemi.Models.Musteri;

namespace KurumTahsilatSistemi.MachineLearning.Services
{
    public class OdemeRiskTahminService
        : IOdemeRiskTahminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public OdemeRiskTahminService(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<MusteriRiskSonucu>>
            TumMusterileriAnalizEtAsync(
                CancellationToken cancellationToken = default)
        {
            var musteriler = await _context.Musteriler
                .AsNoTracking()
                .Include(x => x.Borclar)
                    .ThenInclude(x => x.Tahsilatlar)
                .Include(x => x.Borclar)
                    .ThenInclude(x => x.TahsilatUrunu)
                        .ThenInclude(x => x.Kurum)
                .OrderBy(x => x.AdSoyad)
                .ToListAsync(cancellationToken);

            var sonuclar = new List<MusteriRiskSonucu>();

            foreach (var musteri in musteriler)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sonuc = MusteriTahminiOlustur(musteri);

                sonuclar.Add(sonuc);
            }

            return sonuclar;
        }

        public async Task<MusteriRiskSonucu?>
            MusteriyiAnalizEtAsync(
                int musteriId,
                CancellationToken cancellationToken = default)
        {
            var musteri = await _context.Musteriler
                .AsNoTracking()
                .Include(x => x.Borclar)
                    .ThenInclude(x => x.Tahsilatlar)
                .Include(x => x.Borclar)
                    .ThenInclude(x => x.TahsilatUrunu)
                        .ThenInclude(x => x.Kurum)
                .FirstOrDefaultAsync(
                    x => x.Id == musteriId,
                    cancellationToken);

            if (musteri is null)
            {
                return null;
            }

            return MusteriTahminiOlustur(musteri);
        }

        private MusteriRiskSonucu MusteriTahminiOlustur(
             MusteriEntity musteri)
        {
            var modelDosyaYolu = Path.Combine(
                _environment.ContentRootPath,
                "MachineLearning",
                "ModelsZip",
                "odeme-risk-modeli.zip");

            var sonuc = new MusteriRiskSonucu
            {
                MusteriId = musteri.Id,
                MusteriNo = musteri.MusteriNo,
                AdSoyad = musteri.AdSoyad,
                MusteriTipi =
                    MusteriOdemeService
                        .MusteriTipiniBelirle(musteri),
                ToplamBorcSayisi = musteri.Borclar.Count
            };

            /*
             * Model dosyası henüz oluşturulmadıysa
             * müşteriye tahmin uygulanamaz.
             */
            if (!File.Exists(modelDosyaYolu))
            {
                sonuc.TahminYapildiMi = false;
                sonuc.Aciklama =
                    "Eğitilmiş model dosyası bulunamadı.";

                return sonuc;
            }

            var basariliTahsilatiBulunanBorclar =
                musteri.Borclar
                    .Select(x => new
                    {
                        Borc = x,

                        Tahsilat = x.Tahsilatlar
                            .Where(t =>
                                t.IslemDurumu == "Başarılı")
                            .OrderBy(t =>
                                t.IslemTarihi)
                            .FirstOrDefault()
                    })
                    .Where(x => x.Tahsilat is not null)
                    .ToList();

            sonuc.ZamanindaOdenenBorcSayisi =
                basariliTahsilatiBulunanBorclar.Count(x =>
                    x.Tahsilat!.IslemTarihi.Date
                    <= x.Borc.SonOdemeTarihi.Date);

            sonuc.GecikmeliOdenenBorcSayisi =
                basariliTahsilatiBulunanBorclar.Count(x =>
                    x.Tahsilat!.IslemTarihi.Date
                    > x.Borc.SonOdemeTarihi.Date);

            var odenmisBorcSayisi =
                sonuc.ZamanindaOdenenBorcSayisi +
                sonuc.GecikmeliOdenenBorcSayisi;

            sonuc.GecmisZamanindaOdemeOrani =
     odenmisBorcSayisi == 0
         ? 0.50
         : (double)sonuc.ZamanindaOdenenBorcSayisi /
           odenmisBorcSayisi;

            var odenmemisBorclar = musteri.Borclar
                .Where(x => x.Durum == "Ödenmedi")
                .OrderBy(x => x.SonOdemeTarihi)
                .ToList();

            sonuc.OdenmemisBorcSayisi =
                odenmemisBorclar.Count;

            sonuc.ToplamOdenmemisTutar =
                odenmemisBorclar.Sum(x => x.BorcTutari);

            /*
             * Ödenmemiş borcu olmayan müşteride tahmin
             * yapılacak aktif bir kayıt bulunmaz.
             */
            if (odenmemisBorclar.Count == 0)
            {
                sonuc.TahminYapildiMi = false;
                sonuc.Aciklama =
                    "Müşterinin ödenmemiş borcu bulunmamaktadır.";

                return sonuc;
            }

            var mlContext = new MLContext(seed: 2026);

            var model = mlContext.Model.Load(
                modelDosyaYolu,
                out _);

            /*
             * Bu servis metodu tek istek içerisinde seri
             * çalıştığı için PredictionEngine kullanılabilir.
             * Daha sonra yüksek trafik için
             * PredictionEnginePool kullanılabilir.
             */
            var predictionEngine =
                mlContext.Model.CreatePredictionEngine<
                    OdemeRiskModelInput,
                    OdemeRiskModelOutput>(model);

            var oncekiBorcSayisi =
                basariliTahsilatiBulunanBorclar.Count;

            var oncekiZamanindaOdemeSayisi =
                sonuc.ZamanindaOdenenBorcSayisi;

            var oncekiGecikmeliOdemeSayisi =
                sonuc.GecikmeliOdenenBorcSayisi;

            var zamanindaOdemeOrani =
                oncekiBorcSayisi == 0
                    ? 0.50f
                    : (float)oncekiZamanindaOdemeSayisi /
                      oncekiBorcSayisi;

            var gecikmeliOdemeler =
                basariliTahsilatiBulunanBorclar
                    .Where(x =>
                        x.Tahsilat!.IslemTarihi.Date
                        > x.Borc.SonOdemeTarihi.Date)
                    .ToList();

            var ortalamaGecikmeGunu =
                gecikmeliOdemeler.Count == 0
                    ? 0f
                    : (float)gecikmeliOdemeler
                        .Average(x =>
                            (x.Tahsilat!.IslemTarihi.Date
                             - x.Borc.SonOdemeTarihi.Date)
                            .TotalDays);

            var ortalamaGecmisBorcTutari =
                musteri.Borclar.Count == 0
                    ? 0f
                    : (float)musteri.Borclar
                        .Average(x => x.BorcTutari);

            foreach (var borc in odenmemisBorclar)
            {
                var vadeSuresiGun =
                    Math.Max(
                        0,
                        (borc.SonOdemeTarihi.Date
                         - borc.OlusturmaTarihi.Date)
                        .TotalDays);

                var input = new OdemeRiskModelInput
                {
                    /*
                     * Tahmin sırasında Label kullanılmaz.
                     */
                    ZamanindaOdendiMi = false,

                    BorcTutari =
                        (float)borc.BorcTutari,

                    VadeSuresiGun =
                        (float)vadeSuresiGun,

                    BorcAyi =
                        borc.OlusturmaTarihi.Month,

                    SonOdemeHaftaGunu =
                        (float)borc
                            .SonOdemeTarihi
                            .DayOfWeek,

                    OncekiBorcSayisi =
                        oncekiBorcSayisi,

                    OncekiZamanındaOdemeSayisi =
                        oncekiZamanindaOdemeSayisi,

                    OncekiGecikmeliOdemeSayisi =
                        oncekiGecikmeliOdemeSayisi,

                    OncekiZamanındaOdemeOrani =
                        zamanindaOdemeOrani,

                    OrtalamaGecikmeGunu =
                        ortalamaGecikmeGunu,

                    OrtalamaGecmisBorcTutari =
                        ortalamaGecmisBorcTutari > 0
                            ? ortalamaGecmisBorcTutari
                            : (float)borc.BorcTutari,

                    KurumKodu =
                        borc.TahsilatUrunu
                            .Kurum
                            .KurumKodu,

                    UrunKodu =
                        borc.TahsilatUrunu
                            .UrunKodu,

                    MusteriTipi =
                        sonuc.MusteriTipi
                };

                var tahmin =
                    predictionEngine.Predict(input);

                var olasilik = Math.Clamp(
                    tahmin.Probability,
                    0f,
                    1f);

                sonuc.BorcRiskleri.Add(
                    new BorcRiskSonucu
                    {
                        BorcId = borc.Id,
                        ReferansNo = borc.ReferansNo,
                        KurumAdi =
                            borc.TahsilatUrunu
                                .Kurum
                                .KurumAdi,
                        UrunAdi =
                            borc.TahsilatUrunu
                                .UrunAdi,
                        BorcTutari =
                            borc.BorcTutari,
                        SonOdemeTarihi =
                            borc.SonOdemeTarihi,
                        ZamanindaOdemeOlasiligi =
                            olasilik,
                        RiskSeviyesi =
                            RiskSeviyesiniBelirle(
                                olasilik)
                    });
            }

            /*
             * Müşterinin borçları tutar ağırlıklı
             * ortalamayla hesaplanır.
             *
             * Büyük tutarlı borçlar müşteri risk
             * sonucunda daha fazla etkiye sahip olur.
             */
            var toplamTutar =
                sonuc.BorcRiskleri.Sum(
                    x => x.BorcTutari);

            double ortalamaOlasilik;

            if (toplamTutar > 0)
            {
                ortalamaOlasilik =
                    sonuc.BorcRiskleri.Sum(x =>
                        x.ZamanindaOdemeOlasiligi *
                        (double)x.BorcTutari)
                    / (double)toplamTutar;
            }
            else
            {
                ortalamaOlasilik =
                    sonuc.BorcRiskleri.Average(x =>
                        x.ZamanindaOdemeOlasiligi);
            }

            sonuc.TahminiZamanindaOdemeOlasiligi =
                ortalamaOlasilik;

            sonuc.RiskSeviyesi =
                RiskSeviyesiniBelirle(
                    ortalamaOlasilik);

            sonuc.TahminYapildiMi = true;

            return sonuc;
        }

        private static string RiskSeviyesiniBelirle(
            double zamanindaOdemeOlasiligi)
        {
            if (zamanindaOdemeOlasiligi >= 0.75)
            {
                return "Düşük";
            }

            if (zamanindaOdemeOlasiligi >= 0.45)
            {
                return "Orta";
            }

            return "Yüksek";
        }
    }
}