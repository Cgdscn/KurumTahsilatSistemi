using KurumTahsilatSistemi.MachineLearning.Services;
using KurumTahsilatSistemi.MachineLearning.Training;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurumTahsilatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MakineOgrenmesiController : Controller
    {
        private readonly SentetikOdemeVerisiUretici
            _veriUretici;

        private readonly OdemeRiskModelTrainer
            _modelTrainer;

        private readonly IOdemeRiskTahminService
            _riskTahminService;

        public MakineOgrenmesiController(
            SentetikOdemeVerisiUretici veriUretici,
            OdemeRiskModelTrainer modelTrainer,
            IOdemeRiskTahminService riskTahminService)
        {
            _veriUretici =
                veriUretici;

            _modelTrainer =
                modelTrainer;

            _riskTahminService =
                riskTahminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MusteriRiskleri(
    CancellationToken cancellationToken)
        {
            var sonuclar =
                await _riskTahminService
                    .TumMusterileriAnalizEtAsync(
                        cancellationToken);

            return View(sonuclar);
        }

        [HttpGet]
        public async Task<IActionResult> MusteriRiskDetay(
            int id,
            CancellationToken cancellationToken)
        {
            var sonuc =
                await _riskTahminService
                    .MusteriyiAnalizEtAsync(
                        id,
                        cancellationToken);

            if (sonuc is null)
            {
                return NotFound();
            }

            return View(sonuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VeriSetiOlustur(
            int kayitSayisi = 5000,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var dosyaYolu =
                    await _veriUretici
                        .VeriSetiOlusturAsync(
                            kayitSayisi,
                            cancellationToken);

                TempData["BasariliMesaj"] =
                    $"{kayitSayisi:N0} adet sentetik " +
                    "eğitim kaydı oluşturuldu.";

                TempData["DosyaYolu"] =
                    dosyaYolu;
            }
            catch (ArgumentException exception)
            {
                TempData["HataMesaji"] =
                    exception.Message;
            }
            catch (Exception)
            {
                TempData["HataMesaji"] =
                    "Sentetik eğitim verisi " +
                    "oluşturulurken bir hata meydana geldi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModeliEgit()
        {
            
                var sonuc =
                    _modelTrainer.ModeliEgit();

                TempData["ModelBasarili"] =
                    "Ödeme riski modeli başarıyla eğitildi.";

                TempData["Accuracy"] =
                    sonuc.Accuracy.ToString("P2");

                TempData["Precision"] =
                    sonuc.PositivePrecision.ToString("P2");

                TempData["Recall"] =
                    sonuc.PositiveRecall.ToString("P2");

                TempData["F1"] =
                    sonuc.F1Score.ToString("P2");

                TempData["Auc"] =
                    sonuc.AreUnderRocCurve.ToString("P2");

                TempData["EgitimSayisi"] =
                    sonuc.EgitimKayitSayisi;

                TempData["TestSayisi"] =
                    sonuc.TestKayitSayisi;

                TempData["ModelDosyaYolu"] =
                    sonuc.ModelDosyaYolu;
            
          

            return RedirectToAction(nameof(Index));
        }
    }
}