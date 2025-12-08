namespace ECommerce.Common.ServiceDiscovery.Configuration
{
    // Strongly-typed Consul configuration bound from appsettings.json.
    // This will be used by each microservice.
    public class ConsulConfig
    {
        // Consul server URL, e.g., http://localhost:8500
        public string Address { get; set; } = string.Empty;

        // Unique ID for this service instance, e.g., "order-service-1".
        public string ServiceId { get; set; } = string.Empty;

        // Logical name of the service as seen in Consul, e.g., "OrderService".
        public string ServiceName { get; set; } = string.Empty;

        // Base URL of this instance, e.g., "https://localhost:5021".
        public string ServiceAddress { get; set; } = string.Empty;

        // Relative health endpoint that Consul will call, e.g., "/health".
        public string HealthCheckEndpoint { get; set; } = "/health";

        // Optional tags (version, environment, module, etc.).
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}
