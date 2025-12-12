using ECommerce.Common.ServiceDiscovery.Configuration;
using ECommerce.Common.ServiceDiscovery.Registration;
using ECommerce.Common.ServiceDiscovery.Resolution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Common.ServiceDiscovery.Extensions
{
    // Extension methods for setting up Eureka-based Service Discovery.
    // This is meant to be called from any Microservice or API Gateway,
    // so they can register themselves with Eureka and resolve others.
    public static class EurekaRegistrationExtensions
    {
        // Registers all necessary components for Eureka service discovery
        // and registration into the Dependency Injection (DI) container.
        // services: The IServiceCollection instance (DI container)
        // configuration: Application configuration (to read Eureka settings)
        // returns the updated IServiceCollection (for chaining)
        public static IServiceCollection AddEurekaServiceDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            // STEP 1: Bind "EurekaConfig" section from appsettings.json to a strongly-typed config class.
            // This enables typed access to Eureka settings (like EurekaServer, Instance details, etc.)
            services.Configure<EurekaConfig>(configuration.GetSection("EurekaConfig"));

            // STEP 2: Register IHttpClientFactory
            // This allows Eureka components to make HTTP calls to the Eureka server for:
            // - Registering the service
            // - Sending heartbeats
            // - Fetching other services
            services.AddHttpClient();

            // STEP 3: Register EurekaServiceResolver
            // This provides implementation for IServiceResolver (used across API Gateway and microservices).
            // It handles service resolution (finding healthy service instances via Eureka's REST API).
            services.AddSingleton<IServiceResolver, EurekaServiceResolver>();

            // STEP 4: Register EurekaRegistrationHostedService
            // This background service:
            // - Registers the microservice with Eureka at startup
            // - Sends periodic heartbeats to keep the service marked "UP"
            // - Deregisters the service on application shutdown
            services.AddHostedService<EurekaRegistrationHostedService>();

            // STEP 5: Return the DI container to allow chaining
            return services;
        }
    }
}
