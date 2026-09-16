namespace KurumTahsilatSistemi.Services
{
    public class TahsilatOdemeTalebi
    {
        public int BorcId { get; set;}
        public decimal OdemeTutari { get; set;}

        public string OdemeKanali { get; set;} = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
