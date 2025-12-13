using Serilog.Context;
namespace ProductService.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        // Invoked once per HTTP request. 
        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the existing Correlation ID from headers
            if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                // If not found, create a new one
                correlationId = Guid.NewGuid().ToString("N");
                context.Request.Headers[CorrelationHeader] = correlationId;
            }

            // Make it available in the response header too
            context.Response.Headers[CorrelationHeader] = correlationId;

            // Push into Serilog’s context for automatic inclusion in logs
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }

    // Extension method for registering the CorrelationIdMiddleware
    // in a fluent and readable way inside Program.cs (app.UseCorrelationId()).
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
