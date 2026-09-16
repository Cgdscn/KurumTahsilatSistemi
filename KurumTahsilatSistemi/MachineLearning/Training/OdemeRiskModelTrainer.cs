using KurumTahsilatSistemi.MachineLearning.Models;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;

namespace KurumTahsilatSistemi.MachineLearning.Training
{
    public class OdemeRiskModelTrainer
    {
        private readonly IWebHostEnvironment _environment;

        public OdemeRiskModelTrainer(
            IWebHostEnvironment environment)
        {
            _environment = environment
                ?? throw new ArgumentNullException(
                    nameof(environment));
        }

        public ModelEgitimSonucu ModeliEgit()
        {
            var csvYolu = Path.Combine(
                _environment.ContentRootPath,
                "MachineLearning",
                "Data",
                "odeme-risk-egitim.csv");

            if (!File.Exists(csvYolu))
            {
                throw new FileNotFoundException(
                    "Eğitim CSV dosyası bulunamadı.",
                    csvYolu);
            }

            var dosyaBilgisi = new FileInfo(csvYolu);

            if (dosyaBilgisi.Length == 0)
            {
                throw new InvalidOperationException(
                    "Eğitim CSV dosyası boştur.");
            }

            var mlContext = new MLContext(seed: 2026);

            IDataView veri;

            try
            {
                veri = mlContext.Data
                    .LoadFromTextFile<OdemeRiskModelInput>(
                        path: csvYolu,
                        hasHeader: true,
                        separatorChar: ',',
                        allowQuoting: true,
                        trimWhitespace: true);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "CSV dosyası okunamadı. " +
                    "Sütun sırası veya veri biçimi hatalı olabilir.",
                    exception);
            }

            var kayitlar = mlContext.Data
                .CreateEnumerable<OdemeRiskModelInput>(
                    veri,
                    reuseRowObject: false)
                .ToList();

            if (kayitlar.Count < 100)
            {
                throw new InvalidOperationException(
                    $"Model eğitimi için yeterli kayıt bulunamadı. " +
                    $"Mevcut kayıt sayısı: {kayitlar.Count}");
            }

            var pozitifSayisi = kayitlar.Count(
                x => x.ZamanindaOdendiMi);

            var negatifSayisi = kayitlar.Count(
                x => !x.ZamanindaOdendiMi);

            if (pozitifSayisi == 0)
            {
                throw new InvalidOperationException(
                    "Veri setinde zamanında ödenmiş kayıt bulunmuyor.");
            }

            if (negatifSayisi == 0)
            {
                throw new InvalidOperationException(
                    "Veri setinde gecikmeli ödenmiş kayıt bulunmuyor.");
            }

            foreach (var kayit in kayitlar)
            {
                kayit.KurumKodu =
                    string.IsNullOrWhiteSpace(kayit.KurumKodu)
                        ? "BILINMIYOR"
                        : kayit.KurumKodu.Trim();

                kayit.UrunKodu =
                    string.IsNullOrWhiteSpace(kayit.UrunKodu)
                        ? "BILINMIYOR"
                        : kayit.UrunKodu.Trim();

                kayit.MusteriTipi =
                    string.IsNullOrWhiteSpace(kayit.MusteriTipi)
                        ? "Bilinmiyor"
                        : kayit.MusteriTipi.Trim();
            }

            /*
             * Temizlenen kayıtlar yeniden IDataView
             * biçimine dönüştürüldü.
             */
            veri = mlContext.Data.LoadFromEnumerable(
                kayitlar);

            var split = mlContext.Data.TrainTestSplit(
                veri,
                testFraction: 0.20,
                seed: 2026);

            var pipeline =
                mlContext.Transforms.Categorical
                    .OneHotEncoding(
                        outputColumnName:
                            "KurumKoduEncoded",
                        inputColumnName:
                            nameof(
                                OdemeRiskModelInput
                                    .KurumKodu))

                .Append(
                    mlContext.Transforms.Categorical
                        .OneHotEncoding(
                            outputColumnName:
                                "UrunKoduEncoded",
                            inputColumnName:
                                nameof(
                                    OdemeRiskModelInput
                                        .UrunKodu)))

                .Append(
                    mlContext.Transforms.Categorical
                        .OneHotEncoding(
                            outputColumnName:
                                "MusteriTipiEncoded",
                            inputColumnName:
                                nameof(
                                    OdemeRiskModelInput
                                        .MusteriTipi)))

                .Append(
                    mlContext.Transforms.Concatenate(
                        "Features",

                        nameof(
                            OdemeRiskModelInput
                                .BorcTutari),

                        nameof(
                            OdemeRiskModelInput
                                .VadeSuresiGun),

                        nameof(
                            OdemeRiskModelInput
                                .BorcAyi),

                        nameof(
                            OdemeRiskModelInput
                                .SonOdemeHaftaGunu),

                        nameof(
                            OdemeRiskModelInput
                                .OncekiBorcSayisi),

                        nameof(
                            OdemeRiskModelInput
                                .OncekiZamanındaOdemeSayisi),

                        nameof(
                            OdemeRiskModelInput
                                .OncekiGecikmeliOdemeSayisi),

                        nameof(
                            OdemeRiskModelInput
                                .OncekiZamanındaOdemeOrani),

                        nameof(
                            OdemeRiskModelInput
                                .OrtalamaGecikmeGunu),

                        nameof(
                            OdemeRiskModelInput
                                .OrtalamaGecmisBorcTutari),

                        "KurumKoduEncoded",
                        "UrunKoduEncoded",
                        "MusteriTipiEncoded"))

                .Append(
                    mlContext.BinaryClassification
                        .Trainers.FastTree(
                            new FastTreeBinaryTrainer.Options
                            {
                                LabelColumnName = "Label",
                                FeatureColumnName = "Features",

                                NumberOfLeaves = 20,

                                NumberOfTrees = 200,

                                MinimumExampleCountPerLeaf = 10,

                                LearningRate = 0.10
                            }));

            ITransformer model;

            try
            {
                model = pipeline.Fit(split.TrainSet);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "FastTree modeli eğitilemedi. " +
                    "Eğitim verilerinde boş veya geçersiz " +
                    "değer bulunabilir.",
                    exception);
            }

            var tahminler = model.Transform(
                split.TestSet);

            var metrikler =
                mlContext.BinaryClassification.Evaluate(
                    tahminler,
                    labelColumnName: "Label",
                    scoreColumnName: "Score",
                    probabilityColumnName: "Probability",
                    predictedLabelColumnName:
                        "PredictedLabel");

            var modelKlasoru = Path.Combine(
                _environment.ContentRootPath,
                "MachineLearning",
                "ModelsZip");

            Directory.CreateDirectory(
                modelKlasoru);

            var modelDosyaYolu = Path.Combine(
                modelKlasoru,
                "odeme-risk-modeli.zip");

            mlContext.Model.Save(
                model,
                split.TrainSet.Schema,
                modelDosyaYolu);

            var egitimSayisi = mlContext.Data
                .CreateEnumerable<OdemeRiskModelInput>(
                    split.TrainSet,
                    reuseRowObject: false)
                .Count();

            var testSayisi = mlContext.Data
                .CreateEnumerable<OdemeRiskModelInput>(
                    split.TestSet,
                    reuseRowObject: false)
                .Count();

            return new ModelEgitimSonucu
            {
                Accuracy = metrikler.Accuracy,

                PositivePrecision =
                    metrikler.PositivePrecision,

                PositiveRecall =
                    metrikler.PositiveRecall,

                F1Score = metrikler.F1Score,

                AreUnderRocCurve =
                    metrikler.AreaUnderRocCurve,

                EgitimKayitSayisi =
                    egitimSayisi,

                TestKayitSayisi =
                    testSayisi,

                ModelDosyaYolu =
                    modelDosyaYolu
            };
        }
    }
}