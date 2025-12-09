namespace APIGateway.ServiceDiscovery
{
    public class YarpConfigRefreshService : BackgroundService
    {
        // The IConfigurationRoot reference allows us to explicitly reload configuration files.
        private readonly IConfigurationRoot _config;

        // Logger used for tracking background refresh activity.
        private readonly ILogger<YarpConfigRefreshService> _logger;

        // Constructor: captures the root configuration and logger.
        public YarpConfigRefreshService(
            IConfiguration configuration,
            ILogger<YarpConfigRefreshService> logger)
        {
            // Cast IConfiguration to IConfigurationRoot so we can call Reload().
            _config = (IConfigurationRoot)configuration;
            _logger = logger;
        }

        // Executes the background task.
        // Periodically triggers configuration reload to ensure YARP is always current.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("YARP configuration auto-refresh service started.");

            // Run until the application stops
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Triggering YARP configuration refresh...");

                    // Forces reverseproxy.json (and other config sources) to be re-read.
                    // This triggers YARP’s IProxyConfigFilter pipeline (e.g., ConsulConfigFilter).
                    _config.Reload();

                    _logger.LogInformation("YARP configuration successfully refreshed from reverseproxy.json and Consul.");

                    // Wait before the next refresh cycle (default: 5 seconds)
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // shutdown (host stopping)
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while refreshing YARP configuration. Retrying soon...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("YARP configuration auto-refresh service stopped.");
        }
    }
}
