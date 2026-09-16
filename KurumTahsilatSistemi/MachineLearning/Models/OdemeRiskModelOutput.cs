using Microsoft.ML.Data;

namespace KurumTahsilatSistemi.MachineLearning.Models
{
    public class OdemeRiskModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool ZamanindaOdenirTahmini { get; set; }

        [ColumnName("Probability")]
        public float Probability { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }
    }
}