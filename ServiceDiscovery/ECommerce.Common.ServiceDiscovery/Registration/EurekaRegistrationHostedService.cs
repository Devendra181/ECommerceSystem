using ECommerce.Common.ServiceDiscovery.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace ECommerce.Common.ServiceDiscovery.Registration
{
    // Background service responsible for the complete Eureka lifecycle: 
    // 1. Register the microservice when the application starts.
    // 2. Send periodic heartbeats so Eureka knows the instance is alive.
    // 3. Deregister cleanly when the application shuts down.
    // This service runs automatically because it inherits from BackgroundService.
    public class EurekaRegistrationHostedService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EurekaConfig _config;
        private readonly ILogger<EurekaRegistrationHostedService> _logger;

        public EurekaRegistrationHostedService(
            IHttpClientFactory httpClientFactory,
            IOptions<EurekaConfig> config,
            ILogger<EurekaRegistrationHostedService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config.Value;
            _logger = logger;
        }

        // This method runs automatically when the Host starts.
        // Workflow:
        // 1. Register the service with Eureka.
        // 2. Enter an infinite loop that sends a heartbeat every X seconds.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting Eureka registration workflow for {_config.ServiceName}");

            try
            {
                await RegisterServiceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Could not register {_config.ServiceName} with Eureka.");
                return; // Without registration, heartbeat must NOT proceed.
            }

            // Begin infinite heartbeat loop
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendHeartbeatAsync(stoppingToken);

                // Wait the configured heartbeat interval before sending the next one
                await Task.Delay(TimeSpan.FromSeconds(_config.HeartbeatIntervalSeconds), stoppingToken);
            }
        }

        // Called when the Host is shutting down.
        // This allows the instance to be removed from Eureka properly.
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deregistering {_config.ServiceName} from Eureka");

            await DeregisterServiceAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }

        // REGISTRATION
        // Registers the microservice instance with the Eureka server.
        // This sends a POST request containing all service metadata like:
        // - instanceId
        // - hostname
        // - port
        // - health URL
        // - status ("UP")
        /// Eureka stores this information and marks the instance as available.
        private async Task RegisterServiceAsync(CancellationToken token)
        {
            _logger.LogInformation($"[REGISTRATION] registered started for: {_config.ServiceName}");

            var client = _httpClientFactory.CreateClient();

            // Detect whether service runs under HTTP or HTTPS.
            string scheme = _config.InstanceHost.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                ? "https"
                : "http";

            // Extract hostname only, removing "http://" or "https://"
            //string cleanHost = _config.InstanceHost
            //    .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
            //    .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
            //    .TrimEnd('/');

            string cleanHost = _config.IPAddress;

            // Eureka requires a unique ID for each running instance
            // This format allows easy human inspection (host:service:port).
            // Keep consistent across services.
            // Example: localhost:OrderService:7082, localhost:PaymentService:7154
            _config.InstanceId = $"{cleanHost}:{_config.ServiceName}:{_config.InstancePort}";

            // Service Loical Name
            string appName = _config.ServiceName;

            // Service Base URL
            string serviceBaseUrl = $"{scheme}://{cleanHost}:{_config.InstancePort}";

            //Eureka Server URL for Registration
            // http://localhost:8761/eureka/apps/OrderService
            string eurekaServerUrl = $"{_config.ServerUrl.TrimEnd('/')}/apps/{appName}";

            // Create payload that follows Eureka's expected registration structure
            // Notes:
            // - port/@enabled true means the HTTP port is the active listener.
            // - securePort/@enabled true means the HTTPS port is the active listener.
            // - home/status/health URLs should be absolute (include scheme/host/port).
            // - leaseInfo controls heartbeat (renewal interval) and eviction window (duration).
            var registration = new
            {
                instance = new
                {
                    instanceId = _config.InstanceId,
                    hostName = cleanHost,
                    app = appName,
                    ipAddr = cleanHost, // You can supply an IP if preferred.
                    status = "UP",
                    overriddenstatus = "UNKNOWN",

                    // Which port block is enabled for http Scheme.
                    port = new Dictionary<string, object>
                    {
                        { "@enabled", scheme == "http" },
                        { "$", _config.InstancePort }
                    },

                    // Which port block is enabled for https Scheme.
                    securePort = new Dictionary<string, object>
                    {
                        { "@enabled", scheme == "https" },
                        { "$", scheme == "https" ? _config.InstancePort : 443 }
                    },

                    // URLs used by monitoring tools, not strictly required by discovery.
                    homePageUrl = $"{serviceBaseUrl}/",
                    statusPageUrl = $"{serviceBaseUrl}/actuator/info",
                    healthCheckUrl = $"{serviceBaseUrl}{_config.HealthCheckPath}",

                    // vipAddress fields are used by some clients for virtual naming;
                    // we mirror the service name.
                    vipAddress = _config.ServiceName,
                    secureVipAddress = _config.ServiceName,

                    // Eureka expects this as part of the payload.
                    countryId = 1,

                    // Required identity block for non-AWS data centers.
                    dataCenterInfo = new Dictionary<string, object>
                    {
                        { "@class", "com.netflix.appinfo.InstanceInfo$DefaultDataCenterInfo" },
                        { "name", "MyOwn" }
                    },

                    // Heartbeat and eviction configuration
                    leaseInfo = new
                    {
                        renewalIntervalInSecs = _config.HeartbeatIntervalSeconds,
                        durationInSecs = _config.LeaseDurationSeconds
                    },

                    // Optional metadata section
                    metadata = _config.Metadata?.ToDictionary(x => x.Key, x => x.Value)
                }
            };

            // Serialize full registration payload
            var json = JsonConvert.SerializeObject(registration, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"[REGISTRATION] Sending registration request to {eurekaServerUrl}");
            _logger.LogDebug($"[REGISTRATION] Registration Payload:\n{json}");

            //Sending Registration Post Request to Eureka
            var response = await client.PostAsync(eurekaServerUrl, content, token);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Log full details for debugging
            _logger.LogTrace($"[REGISTRATION] Registration Response from Eureka : StatusCode {response.StatusCode} | Body: {responseBody}");

            if (response.IsSuccessStatusCode)
                _logger.LogInformation($"[REGISTRATION] {_config.ServiceName} registered successfully");
            else
                _logger.LogError($"[REGISTRATION] Registration failed for {_config.ServiceName}. Status: {response.StatusCode} | {responseBody}");
        }

        // HEARTBEAT
        // Sends a PUT Request to /apps/{APP}/{InstanceId} to renew the lease.
        // If Eureka does not receive heartbeats on time,
        // it marks the instance as DOWN and removes it.
        private async Task SendHeartbeatAsync(CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient();

            string appName = _config.ServiceName;

            // http://localhost:8761/eureka/apps/OrderService/localhost:OrderService:7082
            string eurekaServerUrl = $"{_config.ServerUrl.TrimEnd('/')}/apps/{appName}/{_config.InstanceId}";

            _logger.LogTrace($"[HEARTBEAT] Sending heartbeat to {eurekaServerUrl}");

            try
            {
                // Sending Put Request to Eureka for lease renewal
                var response = await client.PutAsync(eurekaServerUrl, null, token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[HEARTBEAT] Heartbeat OK for {_config.ServiceName}");
                }
                else
                {
                    // Don’t fail the process if a heartbeat misses once; log and try again next interval.
                    // Common causes: temporary network hiccup or Eureka restart.
                    string body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"[HEARTBEAT] Heartbeat FAILED for {_config.ServiceName}. Status: {response.StatusCode} | Body: {body}");
                }
            }
            catch (Exception ex)
            {
                // Network exceptions are expected during restarts. Keep the loop running.
                _logger.LogError(ex, $"[HEARTBEAT] Error while sending heartbeat for {_config.ServiceName}");
            }
        }

        // DEREGISTRATION
        // Removes the service instance from Eureka.
        // Fail silently if the server is already down or unreachable.
        private async Task DeregisterServiceAsync(CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient();

            string appName = _config.ServiceName;

            // http://localhost:8761/eureka/apps/OrderService/localhost:OrderService:7082
            string eurekaServerUrl = $"{_config.ServerUrl.TrimEnd('/')}/apps/{appName}/{_config.InstanceId}";

            _logger.LogInformation($"[DEREGISTER] Sending deregistration request for {_config.ServiceName}");

            try
            {
                var response = await client.DeleteAsync(eurekaServerUrl, token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[DEREGISTER] {_config.ServiceName} deregistered successfully");
                }
                else
                {
                    string body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"[DEREGISTER] Failed to deregister {_config.ServiceName}. Status: {response.StatusCode} | Body: {body}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[DEREGISTER] Exception while deregistering {_config.ServiceName}");
            }
        }
    }
}
