using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NewsApp.Notifications
{
    public class NewsNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NewsNotificationWorker> _logger;
        private readonly NewsNotificationSettings _settings;

        public NewsNotificationWorker(
            IServiceProvider serviceProvider,
            ILogger<NewsNotificationWorker> logger,
            IOptions<NewsNotificationSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News Notification Worker starting...");

            if (!_settings.Enabled)
            {
                _logger.LogInformation("News notifications are disabled. Worker will not run.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();
                    
                    // Esperar el intervalo configurado antes de la próxima verificación
                    var delay = TimeSpan.FromMinutes(_settings.CheckIntervalMinutes);
                    _logger.LogInformation("News Notification Worker: Next check in {Minutes} minutes", 
                        _settings.CheckIntervalMinutes);
                    
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // El servicio está siendo detenido
                    _logger.LogInformation("News Notification Worker is stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in News Notification Worker");
                    
                    // Esperar un poco antes de reintentar en caso de error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("News Notification Worker stopped");
        }

        private async Task DoWorkAsync()
        {
            _logger.LogInformation("News Notification Worker: Starting notification check...");

            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider
                .GetRequiredService<NewsNotificationService>();

            await notificationService.CheckAndSendNotificationsAsync();

            // Verificar si es hora de enviar el resumen diario
            if (_settings.SendDailySummary)
            {
                var currentHour = DateTime.Now.Hour;
                if (currentHour == _settings.DailySummaryHour)
                {
                    _logger.LogInformation("Sending daily summary...");
                    await notificationService.SendDailySummaryAsync();
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("News Notification Worker is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}
