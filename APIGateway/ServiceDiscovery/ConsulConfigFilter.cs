using ECommerce.Common.ServiceDiscovery.Resolution;
using Yarp.ReverseProxy.Configuration;

// Aliases to avoid name clashes with Consul types
using YarpClusterConfig = Yarp.ReverseProxy.Configuration.ClusterConfig;
using YarpDestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using YarpRouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;

namespace APIGateway.ServiceDiscovery
{
    // YARP configuration filter that dynamically populates cluster destinations
    // from Consul using the IConsulServiceResolver abstraction.

    // - Routes stay in reverseproxy.json.
    // - Clusters exist in reverseproxy.json but without hardcoded addresses.
    // - Each cluster includes "ConsulServiceName" metadata, e.g.:
    //   "Clusters": {
    //     "orderCluster": {
    //       "Metadata": {
    //         "ConsulServiceName": "OrderService"
    //       }
    //     }
    //   }

    // - This filter queries Consul (via ConsulServiceResolver)
    //   and fills cluster destinations with healthy service instances.
    public class ConsulConfigFilter : IProxyConfigFilter
    {
        private readonly IConsulServiceResolver _serviceResolver;
        private readonly ILogger<ConsulConfigFilter> _logger;

        public ConsulConfigFilter(
            IConsulServiceResolver serviceResolver,
            ILogger<ConsulConfigFilter> logger)
        {
            _serviceResolver = serviceResolver;
            _logger = logger;
        }

        // This method allows us to modify routes dynamically during YARP configuration loading.
        // We are not altering routes — only clusters — so this method simply returns them as-is.
        public ValueTask<YarpRouteConfig> ConfigureRouteAsync(
            YarpRouteConfig route,
            YarpClusterConfig? cluster,
            CancellationToken cancel)
        {
            // Returning the original route untouched.
            // Routes are static (e.g., /users/{**catch-all} → userCluster).
            return new(route);
        }

        // Dynamically configures the cluster destinations by querying Consul for healthy instances.
        // Each YARP cluster corresponds to a service in Consul.
        public async ValueTask<YarpClusterConfig> ConfigureClusterAsync(
            YarpClusterConfig cluster,
            CancellationToken cancel)
        {
            // Validate if the current cluster has "ConsulServiceName" metadata defined.
            // If not, we skip it since it’s not meant to be resolved via Consul.
            if (cluster.Metadata == null ||
                !cluster.Metadata.TryGetValue("ConsulServiceName", out var serviceName) ||
                string.IsNullOrWhiteSpace(serviceName))
            {
                _logger.LogInformation(
                    "Skipping Consul lookup for cluster {ClusterId} — missing or empty ConsulServiceName metadata.",
                    cluster.ClusterId);

                // Return the cluster unchanged (no destinations added).
                return cluster;
            }

            try
            {
                _logger.LogInformation($"Starting Consul lookup for Service: {serviceName} Cluster: {cluster.ClusterId}");

                // Ask the Consul service resolver to return all healthy URIs for this service.
                // This method internally queries Consul’s /v1/health/service endpoint with passingOnly=true.
                var healthyUris = await _serviceResolver.GetHealthyServiceUrisAsync(serviceName, cancel);

                // If no healthy service instances are found, log it and build an empty destination list.
                if (healthyUris == null || !healthyUris.Any())
                {
                    _logger.LogWarning(
                        $"No healthy instances found in Consul for service {serviceName} Cluster: {cluster.ClusterId}. " +
                        "YARP will have no destinations → Incoming requests will result in HTTP 503. " +
                        "Check if the service is registered with correct Address, Port, and HealthCheck configuration.");

                    // Build an empty cluster to avoid invalid routing entries.
                    var emptyCluster = cluster with
                    {
                        Destinations = new Dictionary<string, YarpDestinationConfig>()
                    };
                    return emptyCluster;
                }

                // Log every healthy instance returned from Consul for diagnostic transparency.
                foreach (var uri in healthyUris)
                {
                    _logger.LogInformation($"Discovered healthy destination for {serviceName}: {uri}");
                }

                // Transform each healthy service URI returned from Consul into a YARP destination.
                // Each destination represents one healthy service instance.
                // We'll build the dictionary manually using a foreach loop for better readability.
                var destinations = new Dictionary<string, YarpDestinationConfig>();

                int index = 1; // Used to generate unique keys for each destination (e.g., UserService-1, UserService-2)

                foreach (var uri in healthyUris)
                {
                    // Construct a unique key for this destination entry.
                    // This key identifies the instance inside the YARP cluster configuration.
                    // Example: "UserService-1", "UserService-2", etc.
                    var destinationKey = $"{serviceName}-{index++}";

                    // Create a YARP destination configuration for this URI.
                    // The Address property is where YARP will forward requests to.
                    var destinationConfig = new YarpDestinationConfig
                    {
                        Address = uri.ToString()
                    };

                    // Add this destination to the dictionary.
                    destinations.Add(destinationKey, destinationConfig);

                    _logger.LogInformation($"Added destination: {destinationKey} → {uri}");
                }

                // Build a new cluster object
                // This updated cluster contains all the dynamically discovered destinations.
                var newCluster = cluster with
                {
                    Destinations = destinations
                };

                _logger.LogInformation(
                    "Cluster {ClusterId} populated with {Count} destinations from Consul for {ServiceName}.",
                    cluster.ClusterId, destinations.Count, serviceName);

                // (Optional) Log which load-balancing strategy YARP will use for this cluster.
                if (!string.IsNullOrEmpty(cluster.LoadBalancingPolicy))
                {
                    _logger.LogInformation(
                        "YARP Load Balancing Policy for cluster {ClusterId}: {Policy}",
                        cluster.ClusterId, cluster.LoadBalancingPolicy);
                }
                else
                {
                    _logger.LogInformation(
                        "No specific load balancing policy set for cluster {ClusterId}. Defaulting to 'PowerOfTwoChoices'.",
                        cluster.ClusterId);
                }

                // Return the new cluster with destinations — YARP will use these for routing.
                return newCluster;
            }
            catch (HttpRequestException httpEx)
            {
                // This usually means Consul couldn’t be reached (network issue, bad URL, SSL problem).
                _logger.LogError(httpEx,
                    $"Network/HTTP error while contacting Consul for {serviceName} (Cluster: {cluster.ClusterId}). " +
                    "Possible causes: Consul agent not reachable, incorrect Consul address, or certificate issues.");

                // Return cluster without destinations — YARP will respond with 503.
                return cluster with { Destinations = new Dictionary<string, YarpDestinationConfig>() };
            }
            catch (Exception ex)
            {
                // This captures unexpected runtime exceptions during the lookup or transformation process.
                _logger.LogError(ex,
                    $"Unexpected error populating cluster {cluster.ClusterId} from Consul for service {serviceName}. " +
                    "Cluster will have no destinations. Investigate inner exception or stack trace for details.");

                return cluster with { Destinations = new Dictionary<string, YarpDestinationConfig>() };
            }
        }
    }
}
