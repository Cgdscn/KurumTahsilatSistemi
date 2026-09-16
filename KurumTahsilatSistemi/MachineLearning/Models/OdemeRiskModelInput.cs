using Microsoft.ML.Data;

namespace KurumTahsilatSistemi.MachineLearning.Models
{
    public class OdemeRiskModelInput
    {
        [LoadColumn(0)]
        [ColumnName("Label")]
        public bool ZamanindaOdendiMi { get; set; }

        [LoadColumn(1)]
        public float BorcTutari { get; set; }

        [LoadColumn(2)]
        public float VadeSuresiGun { get; set; }

        [LoadColumn(3)]
        public float BorcAyi { get; set; }

        [LoadColumn(4)]
        public float SonOdemeHaftaGunu { get; set; }

        [LoadColumn(5)]
        public float OncekiBorcSayisi { get; set; }

        [LoadColumn(6)]
        public float OncekiZamanındaOdemeSayisi { get; set; }

        [LoadColumn(7)]
        public float OncekiGecikmeliOdemeSayisi { get; set; }

        [LoadColumn(8)]
        public float OncekiZamanındaOdemeOrani { get; set; }

        [LoadColumn(9)]
        public float OrtalamaGecikmeGunu { get; set; }

        [LoadColumn(10)]
        public float OrtalamaGecmisBorcTutari { get; set; }

        [LoadColumn(11)]
        public string KurumKodu { get; set; }
            = string.Empty;

        [LoadColumn(12)]
        public string UrunKodu { get; set; }
            = string.Empty;

        [LoadColumn(13)]
        public string MusteriTipi { get; set; }
            = string.Empty;
    }
}