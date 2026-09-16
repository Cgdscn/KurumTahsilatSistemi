using KurumTahsilatSistemi.MachineLearning.Models;
using KurumTahsilatSistemi.Models;
namespace KurumTahsilatSistemi.MachineLearning.Services

{
    public interface IOdemeRiskEgitimVerisiService
    {

        Task<List<OdemeRiskModelInput>>
            EgitimVerileriniOlusturAsync(CancellationToken cancellationToken= default);
    }
}
