namespace KurumTahsilatSistemi.Models
{
    public class AiKullaniciBaglami
    {
        public string KullaniciId { get; set; } =
            string.Empty;

        public int? MusteriId { get; set; }

        public bool AdminMi { get; set; }

        public bool KullaniciAktifMi { get; set; }

        public bool MusteriAktifMi { get; set; }

        public bool IslemYapabilirMi =>
            KullaniciAktifMi &&
            MusteriAktifMi;
    }
}