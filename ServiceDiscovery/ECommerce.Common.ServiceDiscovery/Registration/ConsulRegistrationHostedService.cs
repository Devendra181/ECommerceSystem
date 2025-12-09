using Consul;
using ECommerce.Common.ServiceDiscovery.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Common.ServiceDiscovery.Registration
{
    // This background service is responsible for:
    // 1. Registering the current microservice instance to Consul when the app starts.
    // 2. Deregistering it from Consul during application shutdown.
    public class ConsulRegistrationHostedService : IHostedService
    {
        // The Consul client used to communicate with the Consul agent.
        // Through this, we perform service registration, deregistration, and
        // periodic health check updates.
        private readonly IConsulClient _consulClient;

        // Strongly-typed configuration bound from appsettings.json → ConsulConfig
        // Includes values like:
        // Consul Address
        // Service Name (e.g., "OrderService")
        // Service ID (unique per instance)
        // Service Address (URL)
        // HealthCheck endpoint and metadata tags
        private readonly IOptions<ConsulConfig> _consulOptions;

        // Used for structured logging to track registration and deregistration
        private readonly ILogger<ConsulRegistrationHostedService> _logger;

        // Constructor injection of required dependencies:
        // 1. Consul client for API calls
        // 2. Consul configuration for settings
        // 3. Logger for tracing and observability
        public ConsulRegistrationHostedService(
            IConsulClient consulClient,
            IOptions<ConsulConfig> consulOptions,
            ILogger<ConsulRegistrationHostedService> logger)
        {
            _consulClient = consulClient;
            _consulOptions = consulOptions;
            _logger = logger;
        }

        // This method executes when the .NET host starts the application.
        // It registers the current service instance into Consul.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Load Consul-related settings from configuration (IOptions pattern).
            var config = _consulOptions.Value;

            // ServiceAddress contains the full URL where THIS instance is listening,
            // e.g., "https://localhost:7082" or "http://localhost:5021".
            // We create a Uri object so that we can easily extract the Host and Port.
            var serviceUri = new Uri(config.ServiceAddress);

            // Create the registration (payload) object that will be sent to the Consul agent.
            // This tells Consul:
            // - the logical service name (used by other services to find us e.g "OrderService", "UserService", etc.)
            // - the unique instance ID (so multiple instances of the same service can be tracked)
            // - Network location where this instance is running (host + port)
            // - extra tags/meta information
            // - how to check if we are healthy
            var registration = new AgentServiceRegistration
            {
                // Unique ID for THIS running instance.
                // If you run multiple instances of the same service,
                // each one should have a different ID, even if the Name is the same.
                ID = config.ServiceId,

                // Logical name of the service(shared by all instances of this service).
                // Other services will query Consul using this name.
                Name = config.ServiceName,

                // Network address where THIS instance is reachable.
                // Consul will return this address when another service asks
                // for an instance of ServiceName.
                Address = serviceUri.Host,
                Port = serviceUri.Port,

                // Optional metadata tags. These are often used for:
                //  - environment markers ("dev", "qa", "prod")
                //  - transport ("https")
                //  - version ("v1", "v2")
                Tags = config.Tags,

                // Health check configuration for this instance.
                // Consul will periodically call the provided HTTP endpoint
                // to decide if this instance is healthy or not.
                Check = new AgentServiceCheck
                {
                    // Full health check URL that Consul will call.
                    // Example: "https://localhost:7082/health"
                    HTTP = $"{config.ServiceAddress.TrimEnd('/')}{config.HealthCheckEndpoint}",

                    // How often Consul should call the health endpoint.
                    // Here: every 10 seconds.
                    Interval = TimeSpan.FromSeconds(10),

                    // How long Consul should wait for the health endpoint to respond
                    // before marking the check as failed.
                    Timeout = TimeSpan.FromSeconds(5),

                    // If the health check remains in a "critical" (unhealthy) state
                    // for this duration, Consul will automatically deregister
                    // this service instance so it stops receiving traffic.
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                }
            };

            // Log registration details for visibility
            _logger.LogInformation(
                "Registering {ServiceName} with Consul at {Address}:{Port}",
                registration.Name, registration.Address, registration.Port);

            // STEP 1: 
            // If there is already a registration in Consul with the same ServiceId
            // (for example, a prior crash or unclean shutdown),
            // we explicitly remove it first to avoid stale/duplicate entries.
            await _consulClient.Agent.ServiceDeregister(registration.ID, cancellationToken);

            // STEP 2:
            // Now register THIS instance as a fresh service with Consul.
            // After this call succeeds, other microservices can discover us
            // via the Consul service registry.
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);

            _logger.LogInformation(
                "Service {ServiceName} registered in Consul with ID {ServiceId}",
                registration.Name, registration.ID);
        }

        // Automatically called by the .NET runtime/host during application shutdown.
        // Used to cleanly remove this service instance from Consul's registry.
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Read the same configured ServiceId so we deregister
            // the correct instance from Consul.
            var config = _consulOptions.Value;

            _logger.LogInformation(
                "Deregistering {ServiceName} with ID {ServiceId} from Consul",
                config.ServiceName, config.ServiceId);

            // Inform Consul that this instance is going offline.
            // Once deregistered, other services will no longer receive this instance
            // when they ask Consul for healthy instances of this service.
            await _consulClient.Agent.ServiceDeregister(config.ServiceId, cancellationToken);
        }
    }
}

