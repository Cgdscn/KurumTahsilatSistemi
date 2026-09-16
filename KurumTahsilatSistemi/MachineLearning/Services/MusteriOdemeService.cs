using KurumTahsilatSistemi.Models;
using Microsoft.ML.Data;
using Microsoft.ML;
using Microsoft.ML.FastTree;
using Microsoft.Extensions.ML;
namespace KurumTahsilatSistemi.MachineLearning.Services
{
    public class MusteriOdemeService
    {

        public static string MusteriTipiniBelirle(Musteri? musteri)

        {
            if (musteri is null)
            {
                return "Bilinmiyor";
            }
            if (!string.IsNullOrWhiteSpace(musteri.VergiNo))
            {
                return "Kurumsal";
            }

            if (!string.IsNullOrWhiteSpace(musteri.Tckn))
            {
                return "Bireysel";
            }

            return "Bilinmiyor";
        }
    }
}
