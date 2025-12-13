using Consul;
using Ocelot.Logging;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Consul.Interfaces;

namespace APIGateway.ServiceDiscovery
{
    // Custom builder that tells Ocelot to use the Consul service Address
    // (e.g., "localhost") instead of the node.Name (e.g., "consul-dev")
    public class ConsulServiceBuilder : DefaultConsulServiceBuilder
    {
        public ConsulServiceBuilder(
            IHttpContextAccessor contextAccessor,
            IConsulClientFactory clientFactory,
            IOcelotLoggerFactory loggerFactory)
            : base(contextAccessor, clientFactory, loggerFactory)
        {
        }

        // Use the service's Address as downstream host
        protected override string GetDownstreamHost(ServiceEntry entry, Node node)
        {
            // Prefer the service address if set (localhost, 127.0.0.1, etc.)
            if (!string.IsNullOrWhiteSpace(entry.Service.Address))
            {
                return entry.Service.Address;
            }

            // Fallback to default behavior (node name) if needed
            return base.GetDownstreamHost(entry, node);
        }
    }
}
