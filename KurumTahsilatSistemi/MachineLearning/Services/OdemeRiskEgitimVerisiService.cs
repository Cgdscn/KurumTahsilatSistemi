using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.MachineLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.MachineLearning.Services
{
    public class OdemeRiskEgitimVerisiService
        : IOdemeRiskEgitimVerisiService
    {
        private readonly ApplicationDbContext _context;

        public OdemeRiskEgitimVerisiService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<OdemeRiskModelInput>>
            EgitimVerileriniOlusturAsync(
                CancellationToken cancellationToken =
                    default)
        {
            /*
             * Yalnızca tahsilatı bulunan borçlar eğitim
             * verisi olarak kullanılmaktadır.
             *
             * Çünkü ödenmemiş bir borcun zamanında ödenip
             * ödenmeyeceği henüz kesin olarak bilinmez.
             */
            var borclar = await _context.Borclar
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Include(x => x.TahsilatUrunu)
                    .ThenInclude(x => x.Kurum)
                .Include(x => x.Tahsilatlar)
                .Where(x =>
                    x.Tahsilatlar.Any(t =>
                        t.IslemDurumu == "Başarılı"))
                .OrderBy(x => x.OlusturmaTarihi)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var sonuc =
                new List<OdemeRiskModelInput>();

            foreach (var borc in borclar)
            {
                /*
                 * İlgili borcun geçerli tahsilatı bulunuyor.
                 * İptal edilen kayıt eğitimde kullanılmıyor.
                 */
                var tahsilat = borc.Tahsilatlar
                    .Where(x => x.IslemDurumu == "Başarılı")
                    .OrderBy(x => x.IslemTarihi)
                    .FirstOrDefault();

                if (tahsilat is null)
                {
                    continue;
                }

                /*
                 * Aynı müşterinin yalnızca bu borçtan önceki
                 * kayıtları hesaplamaya katılıyor.
                 *
                 * Böylece gelecekte oluşan bilgilerin geçmiş
                 * tahminlerde kullanılmasının önüne geçiliyor.
                 */
                var oncekiBorclar = borclar
                    .Where(x =>
                        x.MusteriId == borc.MusteriId &&
                        x.Id != borc.Id &&
                        x.OlusturmaTarihi
                            < borc.OlusturmaTarihi)
                    .ToList();

                var oncekiOdemeKayitlari =
                    oncekiBorclar
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
                        .Where(x =>
                            x.Tahsilat is not null)
                        .ToList();

                var oncekiBorcSayisi =
                    oncekiOdemeKayitlari.Count;

                var oncekiZamanindaOdemeSayisi =
                    oncekiOdemeKayitlari.Count(x =>
                        x.Tahsilat!.IslemTarihi.Date
                        <= x.Borc.SonOdemeTarihi.Date);

                var oncekiGecikmeliOdemeSayisi =
                    oncekiOdemeKayitlari.Count(x =>
                        x.Tahsilat!.IslemTarihi.Date
                        > x.Borc.SonOdemeTarihi.Date);

                var zamanindaOdemeOrani =
                    oncekiBorcSayisi == 0
                        ? 0.5f
                        : (float)
                          oncekiZamanindaOdemeSayisi
                          / oncekiBorcSayisi;

                var gecikmeliKayitlar =
                    oncekiOdemeKayitlari
                        .Where(x =>
                            x.Tahsilat!.IslemTarihi.Date
                            > x.Borc.SonOdemeTarihi.Date)
                        .ToList();

                var ortalamaGecikmeGunu =
                    gecikmeliKayitlar.Count == 0
                        ? 0f
                        : (float)gecikmeliKayitlar
                            .Average(x =>
                                (x.Tahsilat!.IslemTarihi.Date
                                 - x.Borc.SonOdemeTarihi.Date)
                                .TotalDays);

                var ortalamaGecmisBorcTutari =
                    oncekiBorclar.Count == 0
                        ? (float)borc.BorcTutari
                        : (float)oncekiBorclar
                            .Average(x => x.BorcTutari);

                var vadeSuresi =
                    (borc.SonOdemeTarihi.Date
                     - borc.OlusturmaTarihi.Date)
                    .TotalDays;

                /*
                 * Negatif veya mantıksız vade değerleri
                 * sıfır olarak düzeltiliyor.
                 */
                var vadeSuresiGun =
                    Math.Max(0, vadeSuresi);

                var modelInput =
                    new OdemeRiskModelInput
                    {
                        ZamanindaOdendiMi =
                            tahsilat.IslemTarihi.Date
                            <= borc.SonOdemeTarihi.Date,

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
                            ortalamaGecmisBorcTutari,

                        KurumKodu =
                            borc.TahsilatUrunu
                                .Kurum
                                .KurumKodu,

                        UrunKodu =
                            borc.TahsilatUrunu
                                .UrunKodu,

                        MusteriTipi =
                            MusteriOdemeService
                                .MusteriTipiniBelirle(
                                    borc.Musteri)
                    };

                sonuc.Add(modelInput);
            }

            return sonuc;
        }
    }
}