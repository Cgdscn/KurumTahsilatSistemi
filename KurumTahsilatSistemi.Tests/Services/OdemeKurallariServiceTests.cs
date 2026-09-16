using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;

namespace KurumTahsilatSistemi.Tests.Services
{
    public class OdemeKurallariServiceTests
    {
        private readonly OdemeKurallariService
            _service;

        public OdemeKurallariServiceTests()
        {
            _service =
                new OdemeKurallariService();
        }

        [Fact]
        public void KalanTutariHesapla_KismiOdemeVarsa_DogruSonucDonmeli()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 3_000m,
                durum: "Kısmi Ödendi");

            // Act
            var kalanTutar =
                _service.KalanTutariHesapla(
                    borc);

            // Assert
            Assert.Equal(
                7_000m,
                kalanTutar);
        }

        [Fact]
        public void OdemeyiDogrula_SifirTutar_BasarisizOlmali()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 0m,
                durum: "Ödenmedi");

            // Act
            var sonuc =
                _service.OdemeyiDogrula(
                    borc,
                    0m);

            // Assert
            Assert.False(
                sonuc.BasariliMi);

            Assert.Equal(
                "Ödeme tutarı sıfırdan büyük olmalıdır.",
                sonuc.HataMesaji);
        }

        [Fact]
        public void OdemeyiDogrula_NegatifTutar_BasarisizOlmali()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 0m,
                durum: "Ödenmedi");

            // Act
            var sonuc =
                _service.OdemeyiDogrula(
                    borc,
                    -500m);

            // Assert
            Assert.False(
                sonuc.BasariliMi);
        }

        [Fact]
        public void OdemeyiDogrula_KalanBorctanFazlaOdeme_BasarisizOlmali()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 4_000m,
                durum: "Kısmi Ödendi");

            // Kalan borç 6.000 TL'dir.

            // Act
            var sonuc =
                _service.OdemeyiDogrula(
                    borc,
                    6_001m);

            // Assert
            Assert.False(
                sonuc.BasariliMi);

            Assert.Contains(
                "büyük olamaz",
                sonuc.HataMesaji);
        }

        [Fact]
        public void OdemeyiDogrula_OdenmisBorc_BasarisizOlmali()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 10_000m,
                durum: "Ödendi");

            // Act
            var sonuc =
                _service.OdemeyiDogrula(
                    borc,
                    500m);

            // Assert
            Assert.False(
                sonuc.BasariliMi);

            Assert.Equal(
                "Bu borç daha önce tamamen ödenmiştir.",
                sonuc.HataMesaji);
        }

        [Fact]
        public void OdemeyiDogrula_GecerliKismiOdeme_BasariliOlmali()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 2_000m,
                durum: "Kısmi Ödendi");

            // Act
            var sonuc =
                _service.OdemeyiDogrula(
                    borc,
                    3_000m);

            // Assert
            Assert.True(
                sonuc.BasariliMi);

            Assert.Null(
                sonuc.HataMesaji);
        }

        [Fact]
        public void YeniBorcDurumunuBelirle_BorcTamamlanmiyorsa_KismiOdenmisDonmeli()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 2_000m,
                durum: "Kısmi Ödendi");

            // Act
            var yeniDurum =
                _service.YeniBorcDurumunuBelirle(
                    borc,
                    3_000m);

            // Assert
            Assert.Equal(
                "Kısmi Ödendi",
                yeniDurum);
        }

        [Fact]
        public void YeniBorcDurumunuBelirle_BorcTamamlanirsa_OdendiDonmeli()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 4_000m,
                durum: "Kısmi Ödendi");

            // Act
            var yeniDurum =
                _service.YeniBorcDurumunuBelirle(
                    borc,
                    6_000m);

            // Assert
            Assert.Equal(
                "Ödendi",
                yeniDurum);
        }

        [Fact]
        public void YeniBorcDurumunuBelirle_FazlaTutarVerilseBile_OdendiDonmeli()
        {
            // Arrange
            var borc = BorcOlustur(
                borcTutari: 10_000m,
                odenenTutar: 9_000m,
                durum: "Kısmi Ödendi");

            // Act
            var yeniDurum =
                _service.YeniBorcDurumunuBelirle(
                    borc,
                    2_000m);

            // Assert
            Assert.Equal(
                "Ödendi",
                yeniDurum);
        }

        private static Borc BorcOlustur(
            decimal borcTutari,
            decimal odenenTutar,
            string durum)
        {
            return new Borc
            {
                Id = 1,
                ReferansNo = "TEST-REF-001",
                MusteriNo = "M-001",
                MusteriAdi = "Test Kullanıcısı",
                BorcTutari = borcTutari,
                OdenenTutar = odenenTutar,
                SonOdemeTarihi =
                    DateTime.Today.AddDays(10),
                Durum = durum,
                TahsilatUrunuId = 1
            };
        }
    }
}