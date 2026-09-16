namespace KurumTahsilatSistemi.Services
{
    public interface IVadeBildirimService
    {
        Task KontrolEtAsync(
            CancellationToken cancellationToken = default);
    }
}