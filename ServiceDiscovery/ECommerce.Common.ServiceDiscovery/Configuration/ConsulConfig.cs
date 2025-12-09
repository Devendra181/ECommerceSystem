namespace ECommerce.Common.ServiceDiscovery.Configuration
{
    // Strongly-typed configuration object for Consul integration.
    // Each microservice will bind this class from appsettings.json and use it to:
    // - Know where the Consul server is running.
    // - Describe itself to Consul (ID, Name, Address).
    // - Expose a health-check endpoint for Consul to monitor.
    // - Attach optional tags/metadata (like env, version, https, etc.).
    public class ConsulConfig
    {
        // URL of the Consul server / agent this service will talk to.
        // Example: "http://localhost:8500"
        // This is used when creating the IConsulClient.
        public string Address { get; set; } = string.Empty;

        // Unique identifier for THIS specific instance of the service.
        // Example: "order-service-1" or "order-service-5c3a9d".
        // Multiple instances of the same service will have:
        //  - The same ServiceName
        //  - Different ServiceId values
        public string ServiceId { get; set; } = string.Empty;

        // Logical name of the service as it appears in Consul.
        // Example: "order-service", "product-service", "payment-service".
        // Other microservices will use this name to discover healthy instances.
        public string ServiceName { get; set; } = string.Empty;

        // Public base address where THIS instance is listening.
        // Example: "https://localhost:5021" or "http://orderservice.internal:80".
        // This is used to:
        //  - Build the health-check URL.
        //  - Register Address + Port in Consul (by parsing this URI).
        public string ServiceAddress { get; set; } = string.Empty;

        // Relative path of the health-check endpoint exposed by the service.
        // Consul will periodically call {ServiceAddress}{HealthCheckEndpoint}.
        // Example:
        //  - ServiceAddress: "https://localhost:5021"
        //  - HealthCheckEndpoint: "/health"
        //  -> Final health URL: "https://localhost:5021/health"
        public string HealthCheckEndpoint { get; set; } = "/health";

        // Optional metadata tags for this service instance.
        // Common usages:
        // - "https"      -> to indicate the instance is using HTTPS.
        // - "v1", "v2"   -> versioning for gradual rollouts.
        // - "dev", "qa", "prod" -> environment markers.
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}
