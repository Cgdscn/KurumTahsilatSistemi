using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KurumTahsilatSistemi.Tests.Services
{
    public class VadeBildirimServiceTests
    {
        [Fact]
        public async Task KontrolEtAsync_VadesineUcGunKalanBorc_IcinBildirimOlusturmali()
        {
            await using var context = ContextOlustur();

            var veri = await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(3),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            var bildirim =
                await context.Bildirimler.SingleAsync();

            Assert.Equal(
                veri.Kullanici.Id,
                bildirim.ApplicationUserId);

            Assert.Equal(
                veri.Borc.Id,
                bildirim.BorcId);

            Assert.Equal(
                $"BORC-{veri.Borc.Id}-VADE-3-GUN",
                bildirim.BildirimKodu);

            Assert.Equal(
                "Son Ödeme Tarihi Yaklaşıyor",
                bildirim.Baslik);

            Assert.Equal(
                "Uyarı",
                bildirim.BildirimTipi);

            Assert.False(
                bildirim.OkunduMu);

            Assert.Equal(
                "/Borclar/Borclarim",
                bildirim.YonlendirmeUrl);
        }

        [Fact]
        public async Task KontrolEtAsync_VadesiBugunOlanBorc_IcinBildirimOlusturmali()
        {
            await using var context = ContextOlustur();

            var veri = await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today,
                durum: "Ödenmedi",
                borcTutari: 2_500m,
                odenenTutar: 500m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            var bildirim =
                await context.Bildirimler.SingleAsync();

            Assert.Equal(
                $"BORC-{veri.Borc.Id}-VADE-BUGUN",
                bildirim.BildirimKodu);

            Assert.Equal(
                "Son Ödeme Tarihi Bugün",
                bildirim.Baslik);

            Assert.Contains(
                veri.Borc.ReferansNo,
                bildirim.Mesaj);

            Assert.Contains(
                "son ödeme tarihi bugündür",
                bildirim.Mesaj);
        }

        [Fact]
        public async Task KontrolEtAsync_GecikmisBorc_IcinBildirimOlusturmali()
        {
            await using var context = ContextOlustur();

            var veri = await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(-5),
                durum: "Kısmi Ödendi",
                borcTutari: 10_000m,
                odenenTutar: 4_000m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            var bildirim =
                await context.Bildirimler.SingleAsync();

            Assert.Equal(
                $"BORC-{veri.Borc.Id}-GECIKMIS",
                bildirim.BildirimKodu);

            Assert.Equal(
                "Borcunuzun Vadesi Geçti",
                bildirim.Baslik);

            Assert.Equal(
                "Hata",
                bildirim.BildirimTipi);

            Assert.Contains(
                "5 gündür gecikmiş",
                bildirim.Mesaj);

            Assert.Contains(
                "6.000",
                bildirim.Mesaj);
        }

        [Fact]
        public async Task KontrolEtAsync_AyniBildirimIkinciKezCalistirilirsa_TekKayitOlmali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(3),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();
            await service.KontrolEtAsync();

            var bildirimSayisi =
                await context.Bildirimler.CountAsync();

            Assert.Equal(
                1,
                bildirimSayisi);
        }

        [Fact]
        public async Task KontrolEtAsync_OdenmisBorc_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(-5),
                durum: "Ödendi",
                borcTutari: 1_000m,
                odenenTutar: 1_000m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Empty(
                await context.Bildirimler.ToListAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_KalanTutariSifirOlanBorc_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today,
                durum: "Kısmi Ödendi",
                borcTutari: 1_000m,
                odenenTutar: 1_000m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Equal(
                0,
                await context.Bildirimler.CountAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_VadesineBirGunKalanBorc_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(1),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Empty(
                await context.Bildirimler.ToListAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_VadesineIkiGunKalanBorc_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(2),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Equal(
                0,
                await context.Bildirimler.CountAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_VadesineDortGunKalanBorc_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(4),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Empty(
                await context.Bildirimler.ToListAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_AktifOlmayanKullanici_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            var veri = await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today.AddDays(3),
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            veri.Kullanici.AktifMi = false;

            await context.SaveChangesAsync();

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Equal(
                0,
                await context.Bildirimler.CountAsync());
        }

        [Fact]
        public async Task KontrolEtAsync_KullaniciHesabiOlmayanMusteri_IcinBildirimOlusturmamali()
        {
            await using var context = ContextOlustur();

            var veri = await TestVerileriniEkleAsync(
                context,
                sonOdemeTarihi: DateTime.Today,
                durum: "Ödenmedi",
                borcTutari: 1_000m,
                odenenTutar: 0m);

            context.Users.Remove(
                veri.Kullanici);

            await context.SaveChangesAsync();

            var service = ServiceOlustur(context);

            await service.KontrolEtAsync();

            Assert.Empty(
                await context.Bildirimler.ToListAsync());
        }

        private static ApplicationDbContext ContextOlustur()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(
                        databaseName:
                        Guid.NewGuid().ToString())
                    .EnableSensitiveDataLogging()
                    .Options;

            return new ApplicationDbContext(
                options);
        }

        private static VadeBildirimService ServiceOlustur(
            ApplicationDbContext context)
        {
            return new VadeBildirimService(
                context,
                NullLogger<VadeBildirimService>.Instance);
        }

        private static async Task<TestVerisi> TestVerileriniEkleAsync(
            ApplicationDbContext context,
            DateTime sonOdemeTarihi,
            string durum,
            decimal borcTutari,
            decimal odenenTutar)
        {
            var kurum = new Kurum
            {
                Id = 1,
                KurumAdi = "Test Kurumu",
                KurumKodu = "TEST-KURUM",
                AktifMi = true,
         
            };

            var tahsilatUrunu = new TahsilatUrunu
            {
                Id = 1,
                KurumId = kurum.Id,
                Kurum = kurum,
                UrunAdi = "Test Tahsilat Ürünü",
                UrunKodu = "TEST-URUN",
                
            };

            var musteri = new Musteri
            {
                Id = 1,
                MusteriNo = "MUSTERI-001",
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
                MusteriId = musteri.Id,
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };

            var borc = new Borc
            {
                Id = 1,
                ReferansNo = "REF-001",
                MusteriNo = musteri.MusteriNo,
                MusteriAdi = musteri.AdSoyad,
                BorcTutari = borcTutari,
                OdenenTutar = odenenTutar,
                SonOdemeTarihi = sonOdemeTarihi,
                Durum = durum,

                MusteriId = musteri.Id,
                Musteri = musteri,

                TahsilatUrunuId =
                    tahsilatUrunu.Id,

                TahsilatUrunu =
                    tahsilatUrunu,

                OlusturmaTarihi =
                    DateTime.Now
            };

            context.Kurumlar.Add(kurum);
            context.TahsilatUrunleri.Add(
                tahsilatUrunu);
            context.Musteriler.Add(musteri);
            context.Users.Add(kullanici);
            context.Borclar.Add(borc);

            await context.SaveChangesAsync();

            return new TestVerisi(
                kurum,
                tahsilatUrunu,
                musteri,
                kullanici,
                borc);
        }

        private sealed record TestVerisi(
            Kurum Kurum,
            TahsilatUrunu TahsilatUrunu,
            Musteri Musteri,
            ApplicationUser Kullanici,
            Borc Borc);
    }
}