using APIGateway.Middlewares;

namespace APIGateway.Extensions
{
    // Provides a clean and reusable extension method
    // to register the custom Response Caching Middleware
    // into the ASP.NET Core request processing pipeline.
    public static class ResponseCachingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedisResponseCaching(this IApplicationBuilder builder)
        {
            // Register the custom middleware in the pipeline.
            // When invoked, ASP.NET Core will automatically create and inject
            // all required dependencies (IDistributedCache, IConfiguration, etc.)
            // for the ResponseCachingMiddleware constructor.
            return builder.UseMiddleware<ResponseCachingMiddleware>();
        }
    }
}
