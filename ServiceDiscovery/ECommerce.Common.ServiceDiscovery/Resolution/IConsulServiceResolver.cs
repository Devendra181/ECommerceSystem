namespace ECommerce.Common.ServiceDiscovery.Resolution
{
    // Defines the contract for resolving the URI of a healthy service instance 
    // registered in Consul by its service name.
    public interface IConsulServiceResolver
    {
        // Resolves the base URI of a healthy instance of the specified service.
        // Parameter: serviceName: The name of the target service to locate.
        // Parameter: cancellationToken: Token for cancelling the async operation.
        // returns: The URI of a healthy service instance.
        Task<Uri> ResolveServiceUriAsync(string serviceName, CancellationToken cancellationToken = default);

        // Resolves the base URIs of all healthy instances of the specified service.
        // This is ideal for scenarios like load balancers or gateways that need
        // to know the full set of available instances.
        // Returns: A list of URIs representing all healthy service instances.
        Task<IEnumerable<Uri>> GetHealthyServiceUrisAsync(string serviceName, CancellationToken cancellationToken = default);
    }
}
