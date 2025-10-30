using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace APIGateway.Middlewares
{
    // Edge validator for Bearer tokens:
    // - If an Authorization: Bearer token is present, authenticate it at the gateway.
    // - If invalid/expired, short-circuit with 401 (fail-fast).
    // - If no token, pass through (so public endpoints still work).
    // Per-endpoint authorization remains in downstream services.
    public sealed class GatewayBearerValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _scheme;

        public GatewayBearerValidationMiddleware(RequestDelegate next, string scheme = JwtBearerDefaults.AuthenticationScheme)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _scheme = scheme;
        }

        public async Task InvokeAsync(HttpContext context, IAuthenticationService auth)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var result = await auth.AuthenticateAsync(context, _scheme);

                if (!result.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers["WWW-Authenticate"] =
                        "Bearer error=\"invalid_token\", error_description=\"Invalid or expired access token.\"";
                    context.Response.ContentType = "application/json; charset=utf-8";

                    var problem = new
                    {
                        type = "https://httpstatuses.com/401",
                        title = "Unauthorized",
                        status = 401,
                        detail = "Invalid or expired access token.",
                        traceId = context.TraceIdentifier,
                        timestamp = DateTimeOffset.UtcNow
                    };

                    // At the time of incoming request, As the action method is not executed, means the response has not started yet.
                    if (!context.Response.HasStarted)
                    {
                        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }

                    // IMPORTANT: stop the pipeline on invalid token
                    return;
                }

                // Attach principal for logging/correlation at the edge
                context.User = result.Principal!;
            }

            await _next(context);
        }
    }

    // Registration helper for GatewayBearerValidationMiddleware.
    public static class GatewayBearerValidationExtensions
    {
        public static IApplicationBuilder UseGatewayBearerValidation(
            this IApplicationBuilder app,
            string scheme = JwtBearerDefaults.AuthenticationScheme)
        {
            return app.UseMiddleware<GatewayBearerValidationMiddleware>(scheme);
        }
    }
}
