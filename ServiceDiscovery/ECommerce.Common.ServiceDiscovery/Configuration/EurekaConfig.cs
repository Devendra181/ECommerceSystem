namespace ECommerce.Common.ServiceDiscovery.Configuration
{
    // This class holds all the Eureka-related settings for registering a microservice.
    // These values are typically read from the "EurekaConfig" section in appsettings.json.
    public class EurekaConfig
    {
        // The full base URL of the Eureka server, including "/eureka/" at the end.
        // Example: "http://localhost:8761/eureka/"
        public string ServerUrl { get; set; } = string.Empty;

        // Logical name of your service (used by Eureka to group instances).
        // Example: "OrderService", "ProductService", etc.
        public string ServiceName { get; set; } = string.Empty;

        // Unique ID for this specific service instance.
        // If not set manually, a new GUID will be used.
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();

        // Base URL (host or IP) where the service is running.
        // Example: "http://localhost" or "http://192.168.0.10"
        public string InstanceHost { get; set; } = string.Empty;

        // Base URL (host or IP) where the service is running.
        // Example: "localhost" or "192.168.0.10"
        public string IPAddress { get; set; } = string.Empty;

        // Port on which this service instance listens.
        // Combined with InstanceHost to form the final endpoint.
        public int InstancePort { get; set; }

        // How often (in seconds) the service should send heartbeats to Eureka.
        // Default is 90 seconds. Helps Eureka know this instance is alive.
        public int HeartbeatIntervalSeconds { get; set; } = 90;

        // Time (in seconds) Eureka waits before marking this service instance as DOWN if no heartbeat is received.
        // Should match Eureka's eviction interval.
        public int LeaseDurationSeconds { get; set; } = 300;

        // Relative path used for health checks.
        // Eureka (or monitoring tools) may use this to check if the service is healthy.
        // Example: "/health"
        public string HealthCheckPath { get; set; } = "/health";

        // Optional metadata sent to Eureka for this instance (key-value pairs).
        // Can be used to tag version, environment, or other custom info.
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}

