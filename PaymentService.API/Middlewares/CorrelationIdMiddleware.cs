using Serilog.Context;
namespace PaymentService.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the existing Correlation ID from headers
            if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
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

    // Extension for easy registration
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
