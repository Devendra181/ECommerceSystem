using Consul;

namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // This class:
    //  - Queries Consul's health API for a given service name (e.g. "OrderService").
    //  - Filters out unhealthy instances (only keeps "passing" ones).
    //  - Picks one healthy instance (simple random load-balancing).
    //  - Builds a base Uri (scheme + host + port) that callers can use for HTTP requests.
    public class ConsulServiceResolver : IConsulServiceResolver
    {
        // Low-level Consul client used to talk to the Consul agent (HTTP API).
        // This is injected from DI and is typically a singleton for the whole app.
        private readonly IConsulClient _consulClient;

        // Creates a new resolver that uses the given Consul client.
        // IConsulClient configured to talk to the Consul agent
        // (e.g. Address = http://localhost:8500).
        public ConsulServiceResolver(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        // Resolves a logical Consul service name (e.g. "ProductService") to a base Uri
        // of a **healthy** instance (e.g. https://localhost:7234/).

        // Steps:
        //  1. Query Consul's health API for the given service name.
        //  2. Filter to "passingOnly" instances (health checks are OK).
        //  3. Pick one instance (randomly) for simple load-balancing.
        //  4. Decide scheme (http/https) based on tags (e.g. "https").
        //  5. Build and return a Uri from scheme + address + port.

        // If no healthy instances are found, an InvalidOperationException is thrown.
        public async Task<Uri> ResolveServiceUriAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            // 1. Query Consul's Health API for the given service
            //
            // Health.Service(...) returns:
            //   - All instances of "serviceName" registered in Consul
            //   - Health status for each instance (passing/warning/critical)
            //
            // Parameters:
            //   serviceName   : Logical name like "OrderService", "UserService"
            //   tag           : Filter by tag (we pass empty, so "any tag")
            //   passingOnly   : true → return only instances whose health check is passing
            //   cancellationToken : for request cancellation
            //
            // Result:
            //   queryResult.Response is an array of ServiceEntry objects,
            //   each entry has both Service (address/port/tags) and Checks (health).
            var queryResult = await _consulClient.Health.Service( serviceName, tag: string.Empty, passingOnly: true, cancellationToken);

            var services = queryResult.Response;

            // 2. Ensure at least one healthy instance exists
            // If services is null or empty:
            //   - Either nothing is registered for this service name.
            //   - Or all instances are unhealthy (failed health checks).
            // In both cases, we cannot route a request safely,
            // so we throw an exception and let the caller decide how to handle it.
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
            var index = Random.Shared.Next(services.Length);
            var entry = services[index];
            var service = entry.Service; // Contains Address, Port, Tags, etc.

            // 4. Decide HTTP scheme (http or https) based on Consul tags
            // We use tags on the Consul registration to indicate whether the service
            // should be called over "http" or "https".
            //
            // Example Consul config for a service:
            //   "Tags": [ "orders", "https", "v1" ]
            //
            // If "https" tag is present → use https
            // Otherwise → default to http
            var tags = service.Tags ?? Array.Empty<string>();

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
            return new UriBuilder(scheme, service.Address, service.Port).Uri;
        }
    }
}
