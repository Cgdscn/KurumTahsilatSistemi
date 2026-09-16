using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Services
{
    public class TahsilatService : ITahsilatService
    {
        private readonly ApplicationDbContext _context;

        private readonly IOdemeKurallariService
            _odemeKurallariService;

        private readonly IAuditService
            _auditService;

        public TahsilatService(
            ApplicationDbContext context,
            IOdemeKurallariService odemeKurallariService,
            IAuditService auditService)
        {
            _context = context;

            _odemeKurallariService =
                odemeKurallariService;

            _auditService =
                auditService;
        }

        public async Task<TahsilatIslemSonucu> OdemeYapAsync(
            TahsilatOdemeTalebi talep,
            CancellationToken cancellationToken = default)
        {
            if (talep is null)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Ödeme talebi bulunamadı.");
            }

            if (string.IsNullOrWhiteSpace(
                    talep.ApplicationUserId))
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Ödeme yapan kullanıcı belirlenemedi.");
            }

            var kullanici = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id ==
                         talep.ApplicationUserId,
                    cancellationToken);

            if (kullanici is null)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Ödeme yapan kullanıcı bulunamadı.");
            }

            var kullaniciAdi =
                kullanici.Email
                ?? kullanici.UserName
                ?? kullanici.AdSoyad
                ?? "Bilinmeyen";

            var borc = await _context.Borclar
                .FirstOrDefaultAsync(
                    x => x.Id == talep.BorcId,
                    cancellationToken);

            if (borc is null)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Borç kaydı bulunamadı.");
            }

            var dogrulamaSonucu =
                _odemeKurallariService
                    .OdemeyiDogrula(
                        borc,
                        talep.OdemeTutari);

            if (!dogrulamaSonucu.BasariliMi)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    dogrulamaSonucu.HataMesaji
                    ?? "Ödeme doğrulanamadı.");
            }

            var yeniBorcDurumu =
                _odemeKurallariService
                    .YeniBorcDurumunuBelirle(
                        borc,
                        talep.OdemeTutari);

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            try
            {
                var tahsilat = new Tahsilat
                {
                    BorcId =
                        borc.Id,

                    IslemNo =
                        Guid.NewGuid()
                            .ToString("N")[..16]
                            .ToUpperInvariant(),

                    OdenenTutar =
                        talep.OdemeTutari,

                    IslemTarihi =
                        DateTime.Now,

                    OdemeKanali =
                        talep.OdemeKanali,

                    IslemDurumu =
                        "Başarılı",

                    ApplicationUserId =
                        talep.ApplicationUserId
                };

                borc.OdenenTutar +=
                    talep.OdemeTutari;

                if (borc.OdenenTutar >
                    borc.BorcTutari)
                {
                    borc.OdenenTutar =
                        borc.BorcTutari;
                }

                borc.Durum =
                    yeniBorcDurumu;

                _context.Tahsilatlar.Add(
                    tahsilat);

                await _context.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                try
                {
                    await _auditService.LogAsync(
                        "Tahsilat İşlemi",
                        $"{tahsilat.IslemNo} işlem numarasıyla " +
                        $"{tahsilat.OdenenTutar:N2} TL ödeme yapıldı.",
                        "Başarılı",
                        kullaniciAdi);
                }
                catch
                {
                    /*
                     * Audit log hatası başarılı ödeme
                     * işlemini geri almaz.
                     */
                }

                var mesaj =
                    borc.Durum == "Ödendi"
                        ? "Borç tamamen ödendi."
                        : $"{talep.OdemeTutari:N2} TL tutarında " +
                          "kısmi ödeme başarıyla gerçekleştirildi.";

                return TahsilatIslemSonucu.Basarili(
                    mesaj,
                    tahsilat.Id);
            }
            catch
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return TahsilatIslemSonucu.Basarisiz(
                    "Ödeme işlemi sırasında bir hata oluştu.");
            }
        }

        public async Task<TahsilatIslemSonucu>
            TahsilatIptalEtAsync(
                int tahsilatId,
                string iptalNedeni,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(
                    iptalNedeni))
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "İptal nedeni zorunludur.");
            }

            var tahsilat =
                await _context.Tahsilatlar
                    .Include(x => x.Borc)
                    .Include(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(
                        x => x.Id == tahsilatId,
                        cancellationToken);

            if (tahsilat is null)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Tahsilat kaydı bulunamadı.");
            }

            if (tahsilat.IslemDurumu == "İptal")
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Bu tahsilat işlemi daha önce iptal edilmiştir.");
            }

            if (tahsilat.Borc is null)
            {
                return TahsilatIslemSonucu.Basarisiz(
                    "Tahsilata bağlı borç kaydı bulunamadı.");
            }

            var kullaniciAdi =
                tahsilat.ApplicationUser?.Email
                ?? tahsilat.ApplicationUser?.UserName
                ?? tahsilat.ApplicationUser?.AdSoyad
                ?? "Bilinmeyen";

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            try
            {
                tahsilat.IslemDurumu =
                    "İptal";

                tahsilat.IptalNedeni =
                    iptalNedeni.Trim();

                tahsilat.IptalTarihi =
                    DateTime.Now;

                tahsilat.Borc.OdenenTutar -=
                    tahsilat.OdenenTutar;

                if (tahsilat.Borc.OdenenTutar < 0)
                {
                    tahsilat.Borc.OdenenTutar =
                        0;
                }

                if (tahsilat.Borc.OdenenTutar == 0)
                {
                    tahsilat.Borc.Durum =
                        "Ödenmedi";
                }
                else if (tahsilat.Borc.OdenenTutar <
                         tahsilat.Borc.BorcTutari)
                {
                    tahsilat.Borc.Durum =
                        "Kısmi Ödendi";
                }
                else
                {
                    tahsilat.Borc.Durum =
                        "Ödendi";
                }

                await _context.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                try
                {
                    await _auditService.LogAsync(
                        "Tahsilat İptali",
                        $"{tahsilat.IslemNo} işlem numaralı " +
                        "tahsilat iptal edildi.",
                        "Başarılı",
                        kullaniciAdi);
                }
                catch
                {
                    /*
                     * Audit log hatası iptal işlemini bozmaz.
                     */
                }

                return TahsilatIslemSonucu.Basarili(
                    $"{tahsilat.IslemNo} numaralı " +
                    "tahsilat iptal edildi.",
                    tahsilat.Id);
            }
            catch (Exception hata)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return TahsilatIslemSonucu.Basarisiz(
                    "Tahsilat iptal edilirken hata oluştu: " +
                    hata.Message);
            }
        }
    }
}