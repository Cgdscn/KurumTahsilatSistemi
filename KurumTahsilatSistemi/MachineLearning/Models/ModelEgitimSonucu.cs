namespace KurumTahsilatSistemi.MachineLearning.Models
{
    public class ModelEgitimSonucu
    {

        public double Accuracy {  get; set; }

        public double PositivePrecision {  get; set; }

        public double PositiveRecall { get; set; }

        public double F1Score {  get; set; }

        public double AreUnderRocCurve { get; set; }

        public int  EgitimKayitSayisi {get; set; }  

        public int TestKayitSayisi {  get; set; }

        public string ModelDosyaYolu {  get; set; } = string.Empty;
    }
}
