namespace KurumTahsilatSistemi.Services
{
    public interface ITahsilatService
    {
        Task<TahsilatIslemSonucu> OdemeYapAsync(
            TahsilatOdemeTalebi talep,
            CancellationToken cancellationToken = default);

        Task<TahsilatIslemSonucu> TahsilatIptalEtAsync(
            int tahsilatId,
            string iptalNedeni,
            CancellationToken cancellationToken= default);
    }
}
