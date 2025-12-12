namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // This Provides a unified abstraction for Service Discovery logic.
    // This interface hides the internal details of service registries like
    // Eureka or Consul, so microservices and API Gateway don’t need to worry
    // about which registry is being used under the hood.
    // This helps us switch from one discovery system to another
    // (e.g., Eureka and Consul) without changing consumer code.
    public interface IServiceResolver
    {
        // Resolves and returns the URI of one healthy service instance
        // based on the given logical service name.
        // Ideal for: Direct inter-service HTTP calls (e.g., Order → Product).
        // The selected instance may be chosen randomly from available healthy ones.
        // Example return: http://localhost:5001
        Task<Uri> ResolveServiceUriAsync(string serviceName, CancellationToken cancellationToken = default);

        // Returns a list of all healthy instance URIs for a given service.
        // Ideal for: Load balancers like Ocelot or YARP, which need to
        // build a pool of available destinations for routing.
        // Example return:
        // [
        //     http://localhost:5001,
        //     http://localhost:5002,
        //     http://192.168.1.10:5001
        // ]
        Task<IEnumerable<Uri>> GetHealthyServiceUrisAsync(string serviceName, CancellationToken cancellationToken = default);
    }
}
