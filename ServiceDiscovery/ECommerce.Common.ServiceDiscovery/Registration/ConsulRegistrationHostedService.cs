using Consul;
using ECommerce.Common.ServiceDiscovery.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Common.ServiceDiscovery.Registration
{
    // Background service that automatically:
    //  1. Registers this microservice instance in Consul at startup.
    //  2. Deregisters it from Consul on shutdown.
    // This ensures Consul always has an up-to-date view of which instances are alive.

    public class ConsulRegistrationHostedService : IHostedService
    {
        // Consul HTTP client used to communicate with the Consul agent (register/deregister/service checks).
        private readonly IConsulClient _consulClient;

        // Strongly-typed Consul configuration (Address, ServiceId, ServiceName, ServiceAddress, etc.)
        private readonly IOptions<ConsulConfig> _consulOptions;

        // Logger used for observability and debugging (who registered, on which host/port, etc.).
        private readonly ILogger<ConsulRegistrationHostedService> _logger;

        public ConsulRegistrationHostedService(IConsulClient consulClient, IOptions<ConsulConfig> consulOptions, ILogger<ConsulRegistrationHostedService> logger)
        {
            _consulClient = consulClient;
            _consulOptions = consulOptions;
            _logger = logger;
        }

        // Called by the .NET host when the application starts.
        // This is where we register the service instance with Consul.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Read Consul-related settings bound from appsettings.json (e.g. "Consul" section).
            var config = _consulOptions.Value;

            // ServiceAddress is something like "https://localhost:7082" or "http://localhost:5021".
            // Uri helps us easily extract Host and Port for Consul registration.
            var serviceUri = new Uri(config.ServiceAddress);

            // Build the registration payload that will be sent to Consul.
            // This describes:
            //  - Logical service name ("OrderService", "UserService", etc.)
            //  - Unique instance ID (so multiple instances of the same service can be tracked)
            //  - Network location (host + port)
            //  - Tags (metadata like "https", "v1")
            //  - Health check configuration
            var registration = new AgentServiceRegistration
            {
                // Unique ID for THIS instance.
                // Example: "order-service-1" (you can use GUIDs, machine name, etc.)
                ID = config.ServiceId,

                // Logical name of the service.
                // Other services (and YARP/API Gateway) use this name for discovery.
                Name = config.ServiceName,

                // Host and port where THIS instance is actually listening.
                // Consul will use these values when other services query for this service.
                Address = serviceUri.Host,
                Port = serviceUri.Port,

                // Optional labels for filtering / routing (e.g. "orders", "https", "v1").
                Tags = config.Tags,

                // Health check definition: tells Consul how to periodically verify that this instance is healthy.
                Check = new AgentServiceCheck
                {
                    // Full URL Consul will call to check health.
                    // Example result: "https://localhost:7082/health"
                    HTTP = $"{config.ServiceAddress.TrimEnd('/')}{config.HealthCheckEndpoint}",

                    // How often Consul should call the health endpoint.
                    Interval = TimeSpan.FromSeconds(10),

                    // How long Consul should wait for the health endpoint to respond
                    // before considering the check as failed.
                    Timeout = TimeSpan.FromSeconds(5),

                    // If the service stays in a "critical" (unhealthy) state for this long,
                    // Consul will automatically deregister it.
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                }
            };

            _logger.LogInformation(
                "Registering {ServiceName} with Consul at {Address}:{Port}",
                registration.Name, registration.Address, registration.Port);

            // Safety net:
            // If there is an old/stale registration with the same ID (e.g., process crashed previously),
            // we remove it first to avoid duplicates or ghost instances in Consul.
            await _consulClient.Agent.ServiceDeregister(registration.ID, cancellationToken);

            // Now register the current instance as a fresh service in Consul.
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);

            _logger.LogInformation(
                "Service {ServiceName} registered in Consul with ID {ServiceId}",
                registration.Name, registration.ID);
        }

        // Called by the .NET host when the application is shutting down.
        // This is where we cleanly deregister the service instance from Consul.
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var config = _consulOptions.Value;

            _logger.LogInformation(
                "Deregistering {ServiceName} with ID {ServiceId} from Consul",
                config.ServiceName, config.ServiceId);

            // Tell Consul that this instance is going away.
            // This prevents other services from trying to call a dead instance.
            await _consulClient.Agent.ServiceDeregister(config.ServiceId, cancellationToken);
        }
    }
}
