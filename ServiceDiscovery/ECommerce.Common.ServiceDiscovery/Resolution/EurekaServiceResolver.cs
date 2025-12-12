using ECommerce.Common.ServiceDiscovery.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // EurekaServiceResolver dynamically resolves healthy service instances
    // registered in the Eureka Server.
    // It builds URLs manually using IP/host, port, and scheme

    // This class implements IServiceResolver, allowing it to be used
    // interchangeably with other resolvers like ConsulServiceResolver.
    public class EurekaServiceResolver : IServiceResolver
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EurekaConfig _config;
        private readonly ILogger<EurekaServiceResolver> _logger;

        public EurekaServiceResolver(
            IHttpClientFactory httpClientFactory,
            IOptions<EurekaConfig> config,
            ILogger<EurekaServiceResolver> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config.Value;
            _logger = logger;
        }

        // Returns a single healthy service instance URI for the given service name.
        // - Ideal for direct point-to-point microservice calls.
        // - Uses basic random selection (load balancing) among healthy instances.
        public async Task<Uri> ResolveServiceUriAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            // Ask Eureka for all healthy instances based on Service Name
            var instances = await GetHealthyServiceUrisAsync(serviceName, cancellationToken);

            // If none available, fail fast so the caller can handle it (retry/fallback).
            if (!instances.Any())
                throw new Exception($"No healthy instances found for service '{serviceName}'.");

            // Simple client-side load-balancing: pick one at random.
            var random = new Random();
            var selected = instances.ElementAt(random.Next(instances.Count()));

            _logger.LogInformation($"Resolved {serviceName} → {selected}");
            return selected;
        }

        // Returns all healthy instances registered for the given service.
        // - This is useful for tools like Ocelot or YARP that need a list
        //   of destinations for load balancing or routing.
        // - Manually builds service URLs using scheme (http/https), IP/host, and port.
        public async Task<IEnumerable<Uri>> GetHealthyServiceUrisAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient();

            var requestUrl = $"{_config.ServerUrl.TrimEnd('/')}/apps/{serviceName.ToUpperInvariant()}";

            _logger.LogInformation($"Fetching registered instances for {serviceName} from Eureka...");

            try
            {
                // Create a GET request to Eureka REST API
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Request JSON Payload
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Send the request and receive response
                var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Eureka responded {response.StatusCode} for {serviceName}");
                    return Enumerable.Empty<Uri>();
                }

                // Parse Eureka JSON:
                // {
                //   "application": {
                //     "name": "USERSERVICE",
                //     "instance": { ... } | [ {...}, {...} ]
                //   }
                // }
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var root = JObject.Parse(json);
                var instanceNodes = root["application"]?["instance"];

                // No instances present for this Service Name.
                if (instanceNodes == null)
                {
                    _logger.LogWarning($"No instance nodes found in Eureka for {serviceName}");
                    return Enumerable.Empty<Uri>();
                }

                // Normalize to an enumerable:
                // When Eureka returns a single instance → JObject
                // When multiple → JArray
                IEnumerable<JToken> instances = instanceNodes is JArray jArray
                    ? jArray
                    : new List<JToken> { instanceNodes };

                var uris = new List<Uri>();

                foreach (var instance in instances)
                {
                    // Only consider healthy instances.
                    // Skip instances that are not marked as "UP"
                    var status = instance["status"]?.ToString();
                    if (!string.Equals(status, "UP", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Skipping {Service} instance not UP", serviceName);
                        continue;
                    }

                    // Determine IP/hostname and available ports
                    // Prefer ipAddr, fallback to hostName.
                    var host = instance["ipAddr"]?.ToString() ?? instance["hostName"]?.ToString();

                    // Pull http/https port blocks in Eureka’s schema
                    var portEnabled = instance["port"]?["@enabled"]?.ToObject<bool>() ?? false;
                    var port = instance["port"]?["$"]?.ToObject<int?>();

                    var securePortEnabled = instance["securePort"]?["@enabled"]?.ToObject<bool>() ?? false;
                    var securePort = instance["securePort"]?["$"]?.ToObject<int?>();

                    // Choose scheme and port based on securePort flag
                    // If HTTPS is explicitly enabled, use it; else HTTP.
                    string scheme = securePortEnabled ? "https" : "http";
                    int? selectedPort = securePortEnabled ? securePort : port;

                    // Validate we have both a host and a port to build a URL.
                    if (string.IsNullOrWhiteSpace(host) || !selectedPort.HasValue)
                    {
                        _logger.LogWarning("Invalid host/port info for {Service} instance: {Host}:{Port}", serviceName, host, selectedPort);
                        continue;
                    }

                    // Build a base URI that callers can safely append paths to (ends with a trailing slash).
                    // Example: https://localhost:7269/
                    var uri = new Uri($"{scheme}://{host}:{selectedPort}/");
                    uris.Add(uri);
                }

                _logger.LogInformation($"Discovered {uris.Count} healthy {serviceName} instance(s)");
                return uris;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving {Service} from Eureka", serviceName);
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
