using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KurumTahsilatSistemi.Tests.Services
{
    public class TahsilatServiceTests
    {
        [Fact]
        public async Task OdemeYapAsync_KismiOdeme_TahsilatOlusturupBorcuGuncellemeli()
        {
            await using var ortam = await TestOrtamiOlusturAsync();

            var sonuc = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 3_000m,
                    OdemeKanali = "Kredi Kartı",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.True(sonuc.BasariliMi);
            Assert.NotNull(sonuc.TahsilatId);

            var guncelBorc = await ortam.Context.Borclar
                .SingleAsync(x => x.Id == ortam.Borc.Id);

            var tahsilat = await ortam.Context.Tahsilatlar
                .SingleAsync();

            Assert.Equal(3_000m, guncelBorc.OdenenTutar);
            Assert.Equal("Kısmi Ödendi", guncelBorc.Durum);

            Assert.Equal(3_000m, tahsilat.OdenenTutar);
            Assert.Equal("Başarılı", tahsilat.IslemDurumu);
            Assert.Equal("Kredi Kartı", tahsilat.OdemeKanali);
            Assert.Equal(ortam.Kullanici.Id, tahsilat.ApplicationUserId);
            Assert.False(string.IsNullOrWhiteSpace(tahsilat.IslemNo));
        }

        [Fact]
        public async Task OdemeYapAsync_KalaninTamamiOdenirse_BorcuKapatmali()
        {
            await using var ortam = await TestOrtamiOlusturAsync(
                borcTutari: 10_000m,
                odenenTutar: 4_000m,
                durum: "Kısmi Ödendi");

            var sonuc = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 6_000m,
                    OdemeKanali = "Mobil Bankacılık",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.True(sonuc.BasariliMi);

            var guncelBorc = await ortam.Context.Borclar
                .SingleAsync(x => x.Id == ortam.Borc.Id);

            Assert.Equal(10_000m, guncelBorc.OdenenTutar);
            Assert.Equal("Ödendi", guncelBorc.Durum);
            Assert.Contains("tamamen ödendi", sonuc.Mesaj);
        }

        [Fact]
        public async Task OdemeYapAsync_KalanTutardanFazlaOdeme_VeriyiDegistirmemeli()
        {
            await using var ortam = await TestOrtamiOlusturAsync(
                borcTutari: 10_000m,
                odenenTutar: 4_000m,
                durum: "Kısmi Ödendi");

            var sonuc = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 6_001m,
                    OdemeKanali = "Kredi Kartı",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.False(sonuc.BasariliMi);
            Assert.Contains("büyük olamaz", sonuc.Mesaj);

            var guncelBorc = await ortam.Context.Borclar
                .SingleAsync(x => x.Id == ortam.Borc.Id);

            Assert.Equal(4_000m, guncelBorc.OdenenTutar);
            Assert.Equal("Kısmi Ödendi", guncelBorc.Durum);
            Assert.Empty(await ortam.Context.Tahsilatlar.ToListAsync());
        }

        [Fact]
        public async Task TahsilatIptalEtAsync_TekKismiOdemeIptalEdilirse_BorcYenidenAcikOlmali()
        {
            await using var ortam = await TestOrtamiOlusturAsync();

            var odemeSonucu = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 4_000m,
                    OdemeKanali = "Banka Kartı",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.True(odemeSonucu.BasariliMi);
            Assert.NotNull(odemeSonucu.TahsilatId);

            var iptalSonucu = await ortam.Service.TahsilatIptalEtAsync(
                odemeSonucu.TahsilatId!.Value,
                "Test amacıyla iptal edildi.");

            Assert.True(iptalSonucu.BasariliMi);

            var guncelBorc = await ortam.Context.Borclar
                .SingleAsync(x => x.Id == ortam.Borc.Id);

            var tahsilat = await ortam.Context.Tahsilatlar
                .SingleAsync(x => x.Id == odemeSonucu.TahsilatId.Value);

            Assert.Equal(0m, guncelBorc.OdenenTutar);
            Assert.Equal("Ödenmedi", guncelBorc.Durum);

            Assert.Equal("İptal", tahsilat.IslemDurumu);
            Assert.Equal(
                "Test amacıyla iptal edildi.",
                tahsilat.IptalNedeni);

            Assert.NotNull(tahsilat.IptalTarihi);
        }

        [Fact]
        public async Task TahsilatIptalEtAsync_BirdenFazlaOdemeVarsa_KalanOdemeKorunmali()
        {
            await using var ortam = await TestOrtamiOlusturAsync();

            var birinciOdeme = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 3_000m,
                    OdemeKanali = "Mobil Bankacılık",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            var ikinciOdeme = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = ortam.Borc.Id,
                    OdemeTutari = 2_000m,
                    OdemeKanali = "İnternet Bankacılığı",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.True(birinciOdeme.BasariliMi);
            Assert.True(ikinciOdeme.BasariliMi);

            var iptalSonucu = await ortam.Service.TahsilatIptalEtAsync(
                ikinciOdeme.TahsilatId!.Value,
                "İkinci ödeme hatalı yapıldı.");

            Assert.True(iptalSonucu.BasariliMi);

            var guncelBorc = await ortam.Context.Borclar
                .SingleAsync(x => x.Id == ortam.Borc.Id);

            Assert.Equal(3_000m, guncelBorc.OdenenTutar);
            Assert.Equal("Kısmi Ödendi", guncelBorc.Durum);

            var birinciTahsilat = await ortam.Context.Tahsilatlar
                .SingleAsync(x => x.Id == birinciOdeme.TahsilatId);

            var ikinciTahsilat = await ortam.Context.Tahsilatlar
                .SingleAsync(x => x.Id == ikinciOdeme.TahsilatId);

            Assert.Equal("Başarılı", birinciTahsilat.IslemDurumu);
            Assert.Equal("İptal", ikinciTahsilat.IslemDurumu);
        }

       

        [Fact]
        public async Task OdemeYapAsync_BorcBulunamazsa_BasarisizOlmali()
        {
            await using var ortam = await TestOrtamiOlusturAsync();

            var sonuc = await ortam.Service.OdemeYapAsync(
                new TahsilatOdemeTalebi
                {
                    BorcId = 99999,
                    OdemeTutari = 100m,
                    OdemeKanali = "Nakit",
                    ApplicationUserId = ortam.Kullanici.Id
                });

            Assert.False(sonuc.BasariliMi);
            Assert.Equal("Borç kaydı bulunamadı.", sonuc.Mesaj);
            Assert.Empty(await ortam.Context.Tahsilatlar.ToListAsync());
        }

        [Fact]
        public async Task TahsilatIptalEtAsync_AyniTahsilatIkinciKezIptalEdilememeli()
        {
            await using var ortam =
                await TestOrtamiOlusturAsync();

            var odemeSonucu =
                await ortam.Service.OdemeYapAsync(
                    new TahsilatOdemeTalebi
                    {
                        BorcId =
                            ortam.Borc.Id,

                        OdemeTutari =
                            2_000m,

                        OdemeKanali =
                            "Nakit",

                        ApplicationUserId =
                            ortam.Kullanici.Id
                    });

            Assert.True(
                odemeSonucu.BasariliMi,
                odemeSonucu.Mesaj);

            Assert.NotNull(
                odemeSonucu.TahsilatId);

            var ilkIptal =
                await ortam.Service
                    .TahsilatIptalEtAsync(
                        odemeSonucu.TahsilatId.Value,
                        "İlk iptal.");

            Assert.True(
                ilkIptal.BasariliMi,
                ilkIptal.Mesaj);

            var ikinciIptal =
                await ortam.Service
                    .TahsilatIptalEtAsync(
                        odemeSonucu.TahsilatId.Value,
                        "İkinci iptal.");

            Assert.False(
                ikinciIptal.BasariliMi);

            Assert.Equal(
                "Bu tahsilat işlemi daha önce iptal edilmiştir.",
                ikinciIptal.Mesaj);

            var borc =
                await ortam.Context.Borclar
                    .AsNoTracking()
                    .SingleAsync(x =>
                        x.Id == ortam.Borc.Id);

            Assert.Equal(
                0m,
                borc.OdenenTutar);

            Assert.Equal(
                "Ödenmedi",
                borc.Durum);

            var tahsilat =
                await ortam.Context.Tahsilatlar
                    .AsNoTracking()
                    .SingleAsync(x =>
                        x.Id ==
                        odemeSonucu.TahsilatId.Value);

            Assert.Equal(
                "İptal",
                tahsilat.IslemDurumu);

            Assert.Equal(
                "İlk iptal.",
                tahsilat.IptalNedeni);
        }

        private static async Task<TestOrtami> TestOrtamiOlusturAsync(
            decimal borcTutari = 10_000m,
            decimal odenenTutar = 0m,
            string durum = "Ödenmedi")
        {
            var connection = new SqliteConnection(
                "Data Source=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .EnableSensitiveDataLogging()
                    .Options;

            var context =
                new ApplicationDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var kurum = new Kurum
            {
                KurumAdi = "Test Kurumu",
                KurumKodu =
                    $"KURUM-{Guid.NewGuid():N}",
                AktifMi = true,
            
            };

            var tahsilatUrunu = new TahsilatUrunu
            {
                Kurum = kurum,
                UrunAdi = "Test Ürünü",
                UrunKodu =
                    $"URUN-{Guid.NewGuid():N}",
               
            };

            var musteri = new Musteri
            {
                MusteriNo =
                    $"M-{Guid.NewGuid():N}",
                AdSoyad = "Test Kullanıcısı",
                Tckn = "12345678901",
                Email = "test@example.com",
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };

            var kullanici = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@example.com",
                NormalizedUserName = "TEST@EXAMPLE.COM",
                Email = "test@example.com",
                NormalizedEmail = "TEST@EXAMPLE.COM",
                EmailConfirmed = true,
                AdSoyad = musteri.AdSoyad,
                Musteri = musteri,
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };

            var borc = new Borc
            {
                ReferansNo =
                    $"REF-{Guid.NewGuid():N}",
                MusteriNo = musteri.MusteriNo,
                MusteriAdi = musteri.AdSoyad,

                BorcTutari = borcTutari,
                OdenenTutar = odenenTutar,
                Durum = durum,

                SonOdemeTarihi =
                    DateTime.Today.AddDays(10),

                Musteri = musteri,
                TahsilatUrunu = tahsilatUrunu,

                OlusturmaTarihi = DateTime.Now
            };

            context.AddRange(
                kurum,
                tahsilatUrunu,
                musteri,
                kullanici,
                borc);

            await context.SaveChangesAsync();

            var auditService =
                new FakeAuditService();

            var odemeKurallariService =
                new OdemeKurallariService();

            var tahsilatService =
                new TahsilatService(
                    context,
                    odemeKurallariService,
                    auditService
                     );

            return new TestOrtami(
                connection,
                context,
                tahsilatService,
                kullanici,
                borc);
        }

        private sealed class TestOrtami : IAsyncDisposable
        {
            public TestOrtami(
                SqliteConnection connection,
                ApplicationDbContext context,
                TahsilatService service,
                ApplicationUser kullanici,
                Borc borc)
            {
                Connection = connection;
                Context = context;
                Service = service;
                Kullanici = kullanici;
                Borc = borc;
            }

            public SqliteConnection Connection { get; }

            public ApplicationDbContext Context { get; }

            public TahsilatService Service { get; }

            public ApplicationUser Kullanici { get; }

            public Borc Borc { get; }

            public async ValueTask DisposeAsync()
            {
                await Context.DisposeAsync();
                await Connection.DisposeAsync();
            }
        }
    }
}