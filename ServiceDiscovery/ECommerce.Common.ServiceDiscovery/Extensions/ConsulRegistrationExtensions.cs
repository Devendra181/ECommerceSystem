using Consul;
using ECommerce.Common.ServiceDiscovery.Configuration;
using ECommerce.Common.ServiceDiscovery.Registration;
using ECommerce.Common.ServiceDiscovery.Resolution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Common.ServiceDiscovery.Extensions
{
    // Extension methods to wire up Consul in any ASP.NET Core service (API Gateway or microservice).
    // This does 3 things:
    //  1. Binds Consul settings from appsettings.json into a strongly-typed ConsulConfig.
    //  2. Registers a singleton IConsulClient pointing to the Consul agent.
    //  3. Registers a hosted service that automatically registers/deregisters the service in Consul.

    // Usage (in Program.cs):
    //     builder.Services.AddConsulRegistration(builder.Configuration);

    // Optionally you can pass a different section name:
    //     builder.Services.AddConsulRegistration(builder.Configuration, "ConsulApiGateway");
    public static class ConsulRegistrationExtensions
    {
        // Configures Consul integration for the current service.
        public static IServiceCollection AddConsulRegistration(this IServiceCollection services, IConfiguration configuration, string sectionName = "Consul")
        {
            // 1. Bind Consul configuration section to ConsulConfig (Options pattern)
            // This line:
            //   - Reads configuration from appsettings.json → "Consul" (or custom sectionName).
            //   - Binds it to the ConsulConfig class.
            //   - Registers IOptions<ConsulConfig> in DI.
            //
            // Later, other components (like ConsulRegistrationHostedService) can inject:
            //    IOptions<ConsulConfig> consulOptions
            // to read Address, ServiceId, ServiceName, ServiceAddress, etc.
            services.Configure<ConsulConfig>(configuration.GetSection(sectionName));

            // 2. Register a singleton Consul HTTP client (IConsulClient)
            // We register IConsulClient as a singleton because:
            //  - It internally manages HTTP connections to the Consul agent.
            //  - Creating one shared client per process is the recommended pattern.

            // We use the options we just bound above to configure the client:
            //   - ConsulConfig.Address tells us where the Consul agent is running
            //     (e.g., "http://localhost:8500").

            // Any class that needs to interact with Consul (registration, service discovery, etc.)
            // can now inject IConsulClient via DI.
            services.AddSingleton<IConsulClient>(sp =>
            {
                // Resolve ConsulConfig from the options system.
                var options = sp.GetRequiredService<IOptions<ConsulConfig>>().Value;

                // Create the Consul client using the supplied Consul address.
                // This is typically the local agent URL, NOT the microservice
                // address.
                // Example: http://localhost:8500
                return new ConsulClient(consulConfig =>
                {
                    consulConfig.Address = new Uri(options.Address);
                });
            });

            // 3. Register the hosted service responsible for registration/deregistration
            // ConsulRegistrationHostedService implements IHostedService and:
            //   - On StartAsync: registers this service instance into Consul
            //     with its ServiceName, ServiceId, Address, Port and health check.
            //   - On StopAsync: deregisters the instance so Consul doesn't keep stale entries.
            //
            // By adding it here, we ensure registration happens automatically when
            // the service starts, and cleanup happens on shutdown, without writing
            // extra code in Program.cs.
            services.AddHostedService<ConsulRegistrationHostedService>();

            // Consul-based service resolver for internal HTTP calls
            // IConsulServiceResolver is an abstraction: given a service name like "UserService"
            // it returns a concrete base Uri of a healthy instance (e.g., https://localhost:7269/).
            // ConsulServiceResolver implements this by querying Consul's health API,
            // filtering to passing instances, picking one, and building the Uri.
            // Any place in your code that needs to call another microservice can now depend on
            // IConsulServiceResolver instead of hardcoded URLs.
            services.AddSingleton<IConsulServiceResolver, ConsulServiceResolver>();

            // Allow method chaining.
            return services;
        }
    }
}