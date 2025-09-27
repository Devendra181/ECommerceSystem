using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Interfaces;

namespace NotificationService.Infrastructure.BackgroundJobs
{
    // Background worker that runs periodically and processes queued notifications.
    public sealed class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;       // For structured logging
        private readonly IServiceScopeFactory _scopeFactory;        // To create DI scopes for scoped services
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30); // Interval between runs (30s)

        // onstructor - dependencies are injected by the DI container
        public NotificationWorker(
            ILogger<NotificationWorker> logger,         // Logging dependency
            IServiceScopeFactory scopeFactory)          // Scope factory for resolving scoped services
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // This method is called when the host starts the background service.
        // It will loop until the application stops (or cancellation is requested).
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker started.");

            // Continuous loop - keeps running until application shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new DI scope (since BackgroundService is Singleton,
                    // but we need scoped services like DbContext or NotificationService)
                    using var scope = _scopeFactory.CreateScope();

                    // Resolve the processor (Application implementation of INotificationProcessor)
                    var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();

                    // Process pending notifications (batch of 50 per cycle, skip = 0 for automation)
                    await processor.ProcessQueueBatchAsync(50, 0);
                }
                catch (Exception ex)
                {
                    // Log the error but keep the worker running
                    _logger.LogError(ex, "Error while processing notifications.");
                }

                // Wait 30 seconds before checking again
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
