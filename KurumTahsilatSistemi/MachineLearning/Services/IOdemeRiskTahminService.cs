using KurumTahsilatSistemi.MachineLearning.Models;

namespace KurumTahsilatSistemi.MachineLearning.Services
{
    public interface IOdemeRiskTahminService
    {
        Task<List<MusteriRiskSonucu>>
            TumMusterileriAnalizEtAsync(
                CancellationToken cancellationToken = default);

        Task<MusteriRiskSonucu?>
            MusteriyiAnalizEtAsync(
                int musteriId,
                CancellationToken cancellationToken = default);
    }
}