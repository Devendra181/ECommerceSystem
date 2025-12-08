namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // Abstraction for resolving a service name into a concrete base URI using Consul.
    public interface IConsulServiceResolver
    {
        // Resolves a Consul service name (e.g., "ProductService") into a base URI
        // (e.g., http://localhost:5031) for a healthy instance.
        Task<Uri> ResolveServiceUriAsync(string serviceName, CancellationToken cancellationToken = default);
    }
}
