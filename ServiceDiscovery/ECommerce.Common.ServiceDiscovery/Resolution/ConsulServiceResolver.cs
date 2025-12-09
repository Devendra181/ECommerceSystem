using Consul;
namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // Provides the logic to resolve healthy service instances from Consul's service registry.
    // This class is used by microservices (like Order, Payment, etc.)
    // and gateways to dynamically discover and communicate with other registered services.
    public class ConsulServiceResolver : IConsulServiceResolver
    {
        // Low-level Consul client used to talk to the Consul agent (HTTP API).
        // This is injected from DI and is typically a singleton for the whole app.
        private readonly IConsulClient _consulClient;

        // Initializes the Consul service resolver with the provided Consul client.
        // consulClient: An instance of Consul client for querying Consul for healthy service instances
        public ConsulServiceResolver(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        // Resolves a healthy service instance for the given service name (e.g. "ProductService") to a base Uri.
        // of a **healthy** instance (e.g. https://localhost:7234/).
        // Workflow:
        // 1. Query Consul for services with 'passing' health checks.
        // 2. Ensure at least one instance is available.
        // 3. Randomly select an instance → client-side load balancing.
        // 4. Determine scheme (http/https) based on service tags.
        // 5. Build the final URI from scheme + Host/address + port and return it.
        public async Task<Uri> ResolveServiceUriAsync(
            string serviceName,
            CancellationToken cancellationToken = default)
        {
            //1. Query Consul's Health API for healthy service instances
            //
            // Health.Service(...) returns:
            //   - All instances of "serviceName" registered in Consul
            //   - Health status for each instance (passing/warning/critical)
            //
            var queryResult = await _consulClient.Health.Service(
                serviceName,            // Logical Name of the Service like "OrderService", "UserService"
                tag: string.Empty,      // Optional service tag filter (not used here) (we pass empty, so "any tag")
                passingOnly: true,      // Fetch only passing (healthy) instances
                cancellationToken);     // Cancellation token to cancel the async operation

            // The Response property contains an array of ServiceEntry objects
            // describing all healthy instances of the requested service.
            var services = queryResult.Response;

            // 2. Ensure at least one healthy instance exists
            // If services is null or empty:
            //   - Either nothing is registered for this service name.
            //   - Or all instances are unhealthy (failed health checks).
            // In both cases, we cannot route a request safely,
            // so we throw an exception and let the caller decide how to handle it
            // or
            // If there are no healthy instances, we cannot proceed.
            // Throwing here helps the caller decide how to handle the situation
            // (e.g., retry, fallback, return a friendly error, etc.).
            if (services == null || services.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No healthy instances found for service '{serviceName}'.");
            }

            // 3. Choose one instance (simple random load balancing)
            // When multiple instances are running (e.g. behind a load balancer),
            // Consul can return several healthy entries.
            // Here we:
            //   - Pick a random index between 0 and services.Length - 1.
            //   - This distributes traffic across instances in a simple random fashion.
            // Later, we will use more advanced strategy (round-robin, least-connections, etc.).
            // or
            // Basic load-balancing strategy: random selection.
            // 'services' is an array of healthy service instances returned by Consul.
            // We randomly select one instance to distribute load across all healthy instances.
            var index = Random.Shared.Next(services.Length);

            // Retrieve the randomly selected ServiceEntry from the array.
            // A ServiceEntry contains both service metadata and health check results.
            var entry = services[index];

            // Access the actual service metadata from the selected entry.
            // This includes service name, address (IP/host), port, and tags.
            var service = entry.Service;

            // 4. Decide HTTP scheme (http or https) based on Consul tags
            // We use tags on the Consul registration to indicate whether the service
            // should be called over "http" or "https".
            //
            // Example Consul config for a service:
            //   "Tags": [ "orders", "https", "v1" ]
            //
            // If "https" tag is present → use https
            // Otherwise → default to http
            // or
            // Get the service tags if available; otherwise, use an empty array.
            // Tags can include metadata like "https", "v1", etc.
            var tags = service.Tags ?? Array.Empty<string>();

            // Determine the scheme based on the presence of an "https" tag.
            // If no such tag is found, default to "http".
            var scheme = tags.Any(t =>
                    string.Equals(t, "https", StringComparison.OrdinalIgnoreCase))
                ? "https"
                : "http";

            // 5. Build the Uri from scheme + service address + service port
            // Consul stores:
            //   - service.Address → typically host name or IP (e.g. "localhost")
            //   - service.Port    → listening port (e.g. 7082)
            //
            // We combine these with the scheme to form a base Uri:
            //   "https://localhost:7082/"
            //
            // Callers can then append relative paths:
            //   new Uri(baseUri, "/api/orders")
            // or
            // Build the final URI using the scheme, service address, and port.
            // Example: http://localhost:5001
            var uriBuilder = new UriBuilder(scheme, service.Address, service.Port);

            // return the full service URI(e.g., http://localhost:5001)
            return uriBuilder.Uri;
        }

        // Retrieves all healthy service instances for the given service name.
        // Workflow:
        //  1. Query Consul for healthy ("passing") service instances.
        //  2. Convert all entries into full URIs (http/https + address + port).
        //  3. Return the list for use in load balancing, caching, or metrics.
        public async Task<IEnumerable<Uri>> GetHealthyServiceUrisAsync(
            string serviceName,
            CancellationToken cancellationToken = default)
        {
            // Query Consul Health API for all healthy instances.
            var queryResult = await _consulClient.Health.Service(
                serviceName,
                tag: string.Empty,
                passingOnly: true,
                cancellationToken);

            var services = queryResult.Response;

            // If no healthy instances are available, return an empty collection.
            if (services == null || services.Length == 0)
            {
                return Enumerable.Empty<Uri>();
            }

            // Convert each healthy service entry to a URI (http/https + host + port).
            var uris = services.Select(entry =>
            {
                var service = entry.Service;
                var tags = service.Tags ?? Array.Empty<string>();

                var scheme = tags.Any(t =>
                        string.Equals(t, "https", StringComparison.OrdinalIgnoreCase))
                    ? "https"
                    : "http";

                return new UriBuilder(scheme, service.Address, service.Port).Uri;
            });

            // Return all healthy URIs to the caller.
            return uris;
        }
    }
}
