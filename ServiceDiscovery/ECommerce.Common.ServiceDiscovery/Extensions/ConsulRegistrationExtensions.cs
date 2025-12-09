using Consul;
using ECommerce.Common.ServiceDiscovery.Configuration;
using ECommerce.Common.ServiceDiscovery.Registration;
using ECommerce.Common.ServiceDiscovery.Resolution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Common.ServiceDiscovery.Extensions
{
    // Extension methods to wire-up/plug Consul into any ASP.NET Core service
    // (API Gateway or individual microservice).
    // What this extension does:
    // 1. Reads Consul settings from configuration into ConsulConfig (Options pattern).
    // 2. Creates and registers a singleton IConsulClient that talks to the Consul agent.
    // 3. Registers a hosted service that automatically registers/deregisters
    //     the current service instance in Consul.
    // 4. Enables HttpClientFactory so services can make HTTP calls to other
    //     microservices using Consul-based service discovery.
    // 5. Registers IConsulServiceResolver to resolve healthy service URLs.

    // Usage in Program.cs:
    //    builder.Services.AddConsulRegistration(builder.Configuration);
    // Or with a custom section name:
    //    builder.Services.AddConsulRegistration(builder.Configuration, "ConsulApiGateway");
    public static class ConsulRegistrationExtensions
    {
        // Configures Consul integration for the current service.
        public static IServiceCollection AddConsulRegistration(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "Consul")
        {
            // 1. Bind Consul configuration section to ConsulConfig using the Options pattern.
            // This:
            //  - reads configuration from appsettings.json (or other providers),
            //  - maps the "Consul" section (or custom sectionName) to ConsulConfig,
            //  - registers IOptions<ConsulConfig> in the DI container.

            // Later, any class can inject IOptions<ConsulConfig> to access:
            //   - Address (Consul agent URL)
            //   - ServiceId
            //   - ServiceName
            //   - ServiceAddress
            //   - HealthCheckEndpoint
            //   - Tags
            services.Configure<ConsulConfig>(configuration.GetSection(sectionName));

            // 2. Register a singleton Consul HTTP client (IConsulClient).
            // Why singleton?
            //  - It manages HTTP connections under the hood and is safe to reuse.
            //  - One client per process is the recommended pattern for HTTP-based clients.

            // How is it configured?
            //  - We resolve ConsulConfig from IOptions<ConsulConfig>.
            //  - We set consulConfig.Address to point to the Consul agent
            //    (e.g., "http://localhost:8500").
            services.AddSingleton<IConsulClient>(sp =>
            {
                // Retrieve the current Consul configuration from DI.
                var options = sp.GetRequiredService<IOptions<ConsulConfig>>().Value;

                // Create and configure the Consul client.
                // Note: This address is the Consul agent address, NOT the microservice URL.
                return new ConsulClient(consulConfig =>
                {
                    consulConfig.Address = new Uri(options.Address);
                });
            });

            // 3. Register the hosted service that manages registration/deregistration in Consul.
            // ConsulRegistrationHostedService implements IHostedService and:
            //  - On StartAsync: registers this service instance in Consul with:
            //        ServiceName, ServiceId, Address, Port, Tags, Health Check.
            //  - On StopAsync: cleanly deregisters the instance so Consul
            //        no longer routes traffic to a dead instance.

            // Adding it here means the service automatically participates in service discovery
            // without needing custom code in Program.cs beyond this one extension call.
            services.AddHostedService<ConsulRegistrationHostedService>();

            // 4. Register the shared HttpClientFactory for service-to-service HTTP calls.
            // This integrates perfectly with Consul service discovery.
            // HttpClientFactory:
            //   - Manages connection pooling safely
            //   - Prevents socket exhaustion
            //   - Lets us create HttpClient instances on demand
            //   - Works great with ConsulServiceResolver
            services.AddHttpClient();

            // 5. Register the Consul-based service resolver for dynamic service discovery.
            // Any microservice can now inject IConsulServiceResolver to resolve URLs
            // of other microservices at runtime, instead of using hardcoded URLs.
            services.AddSingleton<IConsulServiceResolver, ConsulServiceResolver>();

            // Return the IServiceCollection to support method chaining in Program.cs.
            return services;
        }
    }
}
