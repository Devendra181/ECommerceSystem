using Serilog.Context;
namespace APIGateway.Middlewares
{
    // Middleware that ensures every incoming request has a unique Correlation ID.
    // This ID is used to trace a request end-to-end across multiple microservices
    // and is automatically attached to all Serilog log entries.
    public class CorrelationIdMiddleware
    {
        // The next middleware in the request pipeline.
        private readonly RequestDelegate _next;

        // The HTTP header name used to carry the correlation ID.
        private const string HeaderName = "X-Correlation-ID";

        // Constructor injects the next delegate in the pipeline.
        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Invoked once per HTTP request. 
        // Responsible for generating or reusing a Correlation ID and injecting it into:
        //      1. The request header (for downstream services)
        //      2. The response header (for clients)
        //      3. The Serilog log context (for structured logging)
        public async Task InvokeAsync(HttpContext context)
        {
            // Try to retrieve the correlation ID from the incoming request headers.
            // If not present, generate a new one.
            if (!context.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
            {
                // Generate a new GUID (without hyphens for compactness)
                cid = Guid.NewGuid().ToString("N");

                // Add the newly generated correlation ID to the request header
                // so that downstream services (like other microservices) can reuse it.
                context.Request.Headers[HeaderName] = cid;
            }

            // Ensure the same correlation ID is returned to the client
            // in the response header, so API consumers can match request/response.
            context.Response.Headers[HeaderName] = cid;

            // LogContext.PushProperty temporarily adds a property to Serilog’s log context.
            // Every log created during this request will automatically include CorrelationId.
            // Example log output:
            // 2025-10-21 10:05:12 [INF] [APIGateway/Development] CorrelationId=8e2f94a9c2d74f7d9bcedff2a742f847
            using (LogContext.PushProperty("CorrelationId", cid.ToString()))
            {
                // Continue request processing by invoking the next middleware.
                await _next(context);
            }
        }
    }

    // Extension method for registering the CorrelationIdMiddleware
    // in a fluent and readable way inside Program.cs (app.UseCorrelationId()).
    public static class CorrelationIdMiddlewareExtensions
    {
        // Adds the CorrelationIdMiddleware to the ASP.NET Core request pipeline.
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            // This allows easy, self-documenting registration in Program.cs:
            // app.UseCorrelationId();
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}