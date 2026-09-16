using System.Globalization;
using KurumTahsilatSistemi.MachineLearning.Models;

namespace KurumTahsilatSistemi.MachineLearning.Training
{
    public class SentetikOdemeVerisiUretici
    {
        private readonly IWebHostEnvironment _environment;

        public SentetikOdemeVerisiUretici(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> VeriSetiOlusturAsync(
            int kayitSayisi = 5000,
            CancellationToken cancellationToken = default)
        {
            if (kayitSayisi < 100)
            {
                throw new ArgumentException(
                    "En az 100 kayıt üretilmelidir.",
                    nameof(kayitSayisi));
            }

            var klasorYolu = Path.Combine(
                _environment.ContentRootPath,
                "MachineLearning",
                "Data");

            Directory.CreateDirectory(klasorYolu);

            var dosyaYolu = Path.Combine(
                klasorYolu,
                "odeme-risk-egitim.csv");

            /*
             * Aynı kod her çalıştırıldığında benzer sonuçlar
             * üretilebilmesi için sabit seed kullanıldı.
             */
            var random = new Random(2026);

            var kurumKodlari = new[]
            {
                "ELEKTRIK",
                "SU",
                "DOGALGAZ",
                "TELEKOM",
                "EGITIM",
                "SIGORTA"
            };

            var urunKodlari = new[]
            {
                "FATURA",
                "TAKSIT",
                "PRIM",
                "ABONELIK",
                "KURUM_ODEME"
            };

            await using var writer = new StreamWriter(
                dosyaYolu,
                false,
                System.Text.Encoding.UTF8);

            /*
             * CSV başlıkları OdemeRiskModelInput
             * özellikleriyle aynı tutuldu.
             */
            await writer.WriteLineAsync(
                "ZamanindaOdendiMi," +
                "BorcTutari," +
                "VadeSuresiGun," +
                "BorcAyi," +
                "SonOdemeHaftaGunu," +
                "OncekiBorcSayisi," +
                "OncekiZamanindaOdemeSayisi," +
                "OncekiGecikmeliOdemeSayisi," +
                "OncekiZamanindaOdemeOrani," +
                "OrtalamaGecikmeGunu," +
                "OrtalamaGecmisBorcTutari," +
                "KurumKodu," +
                "UrunKodu," +
                "MusteriTipi");

            for (var i = 0; i < kayitSayisi; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var riskProfili = RiskProfiliSec(random);

                var musteriTipi = random.NextDouble() < 0.72
                    ? "Bireysel"
                    : "Kurumsal";

                var oncekiBorcSayisi = random.Next(1, 31);

                var temelZamanindaOdemeOrani =
                    riskProfili switch
                    {
                        RiskProfili.Dusuk => RastgeleOndalik(
                            random,
                            0.80,
                            0.98),

                        RiskProfili.Orta => RastgeleOndalik(
                            random,
                            0.50,
                            0.79),

                        RiskProfili.Yuksek => RastgeleOndalik(
                            random,
                            0.10,
                            0.49),

                        _ => 0.50
                    };

                var oncekiZamanindaOdemeSayisi =
                    (int)Math.Round(
                        oncekiBorcSayisi *
                        temelZamanindaOdemeOrani);

                oncekiZamanindaOdemeSayisi =
                    Math.Clamp(
                        oncekiZamanindaOdemeSayisi,
                        0,
                        oncekiBorcSayisi);

                var oncekiGecikmeliOdemeSayisi =
                    oncekiBorcSayisi -
                    oncekiZamanindaOdemeSayisi;

                var gercekZamanindaOdemeOrani =
                    (float)oncekiZamanindaOdemeSayisi /
                    oncekiBorcSayisi;

                var ortalamaGecikmeGunu =
                    riskProfili switch
                    {
                        RiskProfili.Dusuk =>
                            RastgeleOndalik(random, 0, 3),

                        RiskProfili.Orta =>
                            RastgeleOndalik(random, 3, 12),

                        RiskProfili.Yuksek =>
                            RastgeleOndalik(random, 10, 35),

                        _ => 0
                    };

                var ortalamaGecmisBorcTutari =
                    musteriTipi == "Kurumsal"
                        ? RastgeleOndalik(
                            random,
                            5_000,
                            150_000)
                        : RastgeleOndalik(
                            random,
                            250,
                            20_000);

                var borcTutari = YeniBorcTutariOlustur(
                    random,
                    ortalamaGecmisBorcTutari,
                    musteriTipi);

                var vadeSuresiGun = random.Next(7, 61);
                var borcAyi = random.Next(1, 13);
                var sonOdemeHaftaGunu = random.Next(0, 7);

                var kurumKodu =
                    kurumKodlari[
                        random.Next(kurumKodlari.Length)];

                var urunKodu =
                    urunKodlari[
                        random.Next(urunKodlari.Length)];

                var zamanindaOdemeOlasiligi =
                    ZamanindaOdemeOlasiliginiHesapla(
                        gercekZamanindaOdemeOrani,
                        ortalamaGecikmeGunu,
                        borcTutari,
                        ortalamaGecmisBorcTutari,
                        vadeSuresiGun,
                        riskProfili,
                        random);

                var zamanindaOdendiMi =
                    random.NextDouble() <
                    zamanindaOdemeOlasiligi;

                var kayit = new OdemeRiskModelInput
                {
                    ZamanindaOdendiMi =
                        zamanindaOdendiMi,

                    BorcTutari =
                        (float)borcTutari,

                    VadeSuresiGun =
                        vadeSuresiGun,

                    BorcAyi =
                        borcAyi,

                    SonOdemeHaftaGunu =
                        sonOdemeHaftaGunu,

                    OncekiBorcSayisi =
                        oncekiBorcSayisi,

                    OncekiZamanındaOdemeSayisi =
                        oncekiZamanindaOdemeSayisi,

                    OncekiGecikmeliOdemeSayisi =
                        oncekiGecikmeliOdemeSayisi,

                    OncekiZamanındaOdemeOrani =
                        gercekZamanindaOdemeOrani,

                    OrtalamaGecikmeGunu =
                        (float)ortalamaGecikmeGunu,

                    OrtalamaGecmisBorcTutari =
                        (float)ortalamaGecmisBorcTutari,

                    KurumKodu =
                        kurumKodu,

                    UrunKodu =
                        urunKodu,

                    MusteriTipi =
                        musteriTipi
                };

                var csvSatiri = CsvSatiriOlustur(kayit);

                await writer.WriteLineAsync(csvSatiri);
            }

            return dosyaYolu;
        }

        private static RiskProfili RiskProfiliSec(
            Random random)
        {
            var deger = random.NextDouble();

            /*
             * Veri setinin yaklaşık dağılımı:
             *
             * %45 düşük risk
             * %35 orta risk
             * %20 yüksek risk
             */
            if (deger < 0.45)
            {
                return RiskProfili.Dusuk;
            }

            if (deger < 0.80)
            {
                return RiskProfili.Orta;
            }

            return RiskProfili.Yuksek;
        }

        private static double YeniBorcTutariOlustur(
            Random random,
            double ortalamaGecmisBorcTutari,
            string musteriTipi)
        {
            var degisimKatsayisi =
                RastgeleOndalik(
                    random,
                    0.45,
                    1.85);

            var tutar =
                ortalamaGecmisBorcTutari *
                degisimKatsayisi;

            var maksimumTutar =
                musteriTipi == "Kurumsal"
                    ? 300_000
                    : 50_000;

            return Math.Round(
                Math.Clamp(
                    tutar,
                    100,
                    maksimumTutar),
                2);
        }

        private static double
            ZamanindaOdemeOlasiliginiHesapla(
                float zamanindaOdemeOrani,
                double ortalamaGecikmeGunu,
                double borcTutari,
                double ortalamaGecmisBorcTutari,
                int vadeSuresiGun,
                RiskProfili riskProfili,
                Random random)
        {
            /*
             * Başlangıç olasılığı müşterinin geçmiş
             * ödeme davranışından elde edildi.
             */
            var olasilik = zamanindaOdemeOrani;

            /*
             * Ortalama gecikme arttıkça zamanında
             * ödeme olasılığı azaltıldı.
             */
            olasilik -=
                (float)Math.Min(
                    ortalamaGecikmeGunu / 100.0,
                    0.25);

            /*
             * Yeni borç geçmiş ortalamanın çok
             * üzerindeyse risk artırıldı.
             */
            if (ortalamaGecmisBorcTutari > 0)
            {
                var tutarOrani =
                    borcTutari /
                    ortalamaGecmisBorcTutari;

                if (tutarOrani > 1.50)
                {
                    olasilik -= (float)0.10;
                }

                if (tutarOrani > 2.00)
                {
                    olasilik -= (float)0.08;
                }
            }

            /*
             * Daha uzun vade küçük bir olumlu etki
             * oluşturdu.
             */
            if (vadeSuresiGun >= 30)
            {
                olasilik += (float)0.04;
            }

            if (vadeSuresiGun <= 10)
            {
                olasilik -= (float)0.04;
            }

            /*
             * Risk profilinin genel etkisi eklendi.
             */
            olasilik += riskProfili switch
            {
                RiskProfili.Dusuk => 0.07f,
                RiskProfili.Orta => 0f,
                RiskProfili.Yuksek => -0.08f,
                _ => 0
            };

            /*
             * Verilerin tamamen kusursuz ve mekanik
             * görünmemesi için küçük rastgele gürültü
             * eklendi.
             */
            olasilik += (float)RastgeleOndalik(random, -0.08, 0.08);

            return Math.Clamp(
                olasilik,
                0.03,
                0.97);
        }

        private static double RastgeleOndalik(
            Random random,
            double minimum,
            double maksimum)
        {
            return minimum +
                   random.NextDouble() *
                   (maksimum - minimum);
        }

        private static string CsvSatiriOlustur(
            OdemeRiskModelInput kayit)
        {
            var culture =
                CultureInfo.InvariantCulture;

            return string.Join(
                ",",
                kayit.ZamanindaOdendiMi
                    ? "true"
                    : "false",

                kayit.BorcTutari.ToString(culture),

                kayit.VadeSuresiGun.ToString(culture),

                kayit.BorcAyi.ToString(culture),

                kayit.SonOdemeHaftaGunu
                    .ToString(culture),

                kayit.OncekiBorcSayisi
                    .ToString(culture),

                kayit.OncekiZamanındaOdemeSayisi
                    .ToString(culture),

                kayit.OncekiGecikmeliOdemeSayisi
                    .ToString(culture),

                kayit.OncekiZamanındaOdemeOrani
                    .ToString(culture),

                kayit.OrtalamaGecikmeGunu
                    .ToString(culture),

                kayit.OrtalamaGecmisBorcTutari
                    .ToString(culture),

                kayit.KurumKodu,

                kayit.UrunKodu,

                kayit.MusteriTipi);
        }

        private enum RiskProfili
        {
            Dusuk,
            Orta,
            Yuksek
        }
    }
}