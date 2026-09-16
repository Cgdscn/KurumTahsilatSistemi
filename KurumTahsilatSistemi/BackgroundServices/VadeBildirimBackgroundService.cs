using KurumTahsilatSistemi.Services;

namespace KurumTahsilatSistemi.BackgroundServices
{
    public class VadeBildirimBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger
            <VadeBildirimBackgroundService> _logger;

        public VadeBildirimBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<VadeBildirimBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Otomatik vade bildirim servisi başlatıldı.");

            /*
             * Uygulama açıldığında ilk kontrol hemen yapılır.
             */
            await KontrolCalistirAsync(
                stoppingToken);

            /*
             * Saatte bir kontrol edilir.
             *
             * Aynı bildirim BildirimKodu ile engellendiği için
             * saatlik kontrol tekrar bildirim üretmez.
             *
             * Saatlik kontrol seçilmesinin nedeni uygulamanın
             * gün içinde farklı bir saatte açılabilmesidir.
             */
            using var timer =
                new PeriodicTimer(
                    TimeSpan.FromHours(1));

            try
            {
                while (await timer.WaitForNextTickAsync(
                    stoppingToken))
                {
                    await KontrolCalistirAsync(
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                /*
                 * Uygulama kapatılırken normal şekilde
                 * cancellation oluşabilir.
                 */
            }

            _logger.LogInformation(
                "Otomatik vade bildirim servisi durduruldu.");
        }

        private async Task KontrolCalistirAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                /*
                 * BackgroundService singleton'dır.
                 * DbContext scoped olduğu için yeni scope açılır.
                 */
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var vadeBildirimService =
                    scope.ServiceProvider
                        .GetRequiredService
                            <IVadeBildirimService>();

                await vadeBildirimService
                    .KontrolEtAsync(
                        cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken
                    .IsCancellationRequested)
            {
                // Uygulama kapanıyor.
            }
            catch (Exception hata)
            {
                _logger.LogError(
                    hata,
                    "Otomatik vade kontrolü sırasında hata oluştu.");
            }
        }
    }
}