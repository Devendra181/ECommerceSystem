using APIGateway.Models;
using APIGateway.Services;
using APIGateway.Middlewares;

namespace APIGateway.Extensions
{
    // Provides extension methods to register and enable 
    // custom in-memory rate limiting within the API Gateway.
    public static class RateLimitingExtensions
    {
        // Adds and configures the custom rate limiting services to the dependency injection container.
        public static IServiceCollection AddCustomRateLimiting(
            this IServiceCollection services, IConfiguration configuration)
        {
            // 1️. Bind "RateLimiting" section in appsettings.json → RateLimitSettings model
            // Example:
            // "RateLimiting": {
            //   "IsEnabled": true,
            //   "DefaultPolicy": { ... },
            //   "ProductApiPolicy": { ... }
            // }
            services.Configure<RateLimitSettings>(
                configuration.GetSection("RateLimiting"));

            // 2️. Register the policy service that creates limiters based on configuration
            // This allows the middleware to request a RateLimiter via IRateLimitPolicyService
            services.AddSingleton<IRateLimitPolicyService, RateLimitPolicyService>();

            // Why Singleton?
            //  Rate limiting depends on shared in-memory limiters.
            //  A singleton ensures consistent throttling across requests.

            // Return for fluent chaining in Program.cs
            return services;
        }

        // Adds the "RateLimitingMiddleware" into the application request pipeline.
        // What this does:
        // Inserts your custom rate-limiting logic before controller execution.
        // Every request will pass through this middleware where it will either:
        //    Be allowed → proceed to next middleware or controller, OR
        //    Be throttled → immediately return HTTP 429 Too Many Requests.

        // Recommended Placement:
        // Place this middleware early in the pipeline (after authentication but before business logic).
        public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
        {
            // Insert custom middleware into the ASP.NET Core pipeline
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
