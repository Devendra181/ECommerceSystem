using APIGateway.Models;
using APIGateway.Services;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace APIGateway.Middlewares
{
    // This middleware enforces rate limiting for every incoming request:
    // For authenticated users → limits are applied per UserId (from JWT).
    // For anonymous users → limits are applied per IP address.

    // Workflow:
    //  1️. Detect which API endpoint is being accessed (Product, Order, Payment, etc.).
    //  2️. Resolve identity (UserId or IP).
    //  3️. Retrieve or create a RateLimiter (based on policy).
    //  4️. Attempt to acquire permission (token) from the limiter.
    //  5️. If denied → return HTTP 429 Too Many Requests.
    //  6️. Otherwise → continue down the pipeline.
    public class RateLimitingMiddleware
    {
        // Points to the next middleware in the pipeline
        private readonly RequestDelegate _next;

        // A dependency that creates the correct RateLimiter 
        private readonly IRateLimitPolicyService _policyService;

        // An in-memory thread-safe dictionary that stores a separate limiter per user/IP per policy
        private static readonly ConcurrentDictionary<string, RateLimiter> _limiters = new();

        public RateLimitingMiddleware(RequestDelegate next, IRateLimitPolicyService policyService)
        {
            _next = next;
            _policyService = policyService;
        }

        // This is called automatically by the ASP.NET Core pipeline for every HTTP request.
        public async Task InvokeAsync(HttpContext context)
        {
            // Step 1: Identify the Request Path
            // Extracts the URL path, e.g. /api/products/get-all.
            // Helps the middleware determine which API category the request belongs to.
            var path = context.Request.Path.Value ?? string.Empty;

            // Step 2: Resolve the Request Identity (IP or User Id)
            // This method determines who is making the request.
            var identityKey = ResolveIdentity(context);

            // Step 3: Determine Which API Policy Applies
            // Product, Order, Payment, etc.
            var policy = GetPolicyFromPath(path);

            // Step 4: Combine both to form a unique limiter key
            // Builds a unique key combining policy + user/IP
            // Example: "OrderApi_user:123" or "Default_ip:203.91.45.10"
            var limiterKey = $"{policy}_{identityKey}";

            // Step 5: Retrieve or Create a Limiter
            // Looks up an existing limiter in the dictionary.
            // If not found, creates a new one via RateLimitPolicyService.
            var limiter = _limiters.GetOrAdd(
                limiterKey,
                _ => _policyService.CreateRateLimiter(policy));

            // Step 6: Try to Acquire a Permit
            // This is the actual enforcement line.
            // Requests 1 token(permit) from the limiter.
            // The limiter(either FixedWindow or Concurrency) checks:
            // Has this user already hit their request limit?
            // If not → allow and mark a token as used.
            // If yes → deny access.
            // The lease object tells you if the request was accepted:
            //      lease.IsAcquired == true → proceed.
            //      lease.IsAcquired == false → reject(too many requests).
            using var lease = await limiter.AcquireAsync(1, context.RequestAborted);

            // Step 6: Handle Rejected Requests
            if (!lease.IsAcquired)
            {
                // Responds with HTTP 429 Too Many Requests.
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                // Adds a standard Retry-After header to inform the client when to retry.
                context.Response.Headers["Retry-After"] = "60";

                await context.Response.WriteAsync(
                    "{\"error\":\"rate_limit_exceeded\",\"message\":\"Too many requests. Please try again later.\"}");

                // Stops the request pipeline here — the request doesn’t reach your downstream APIs.
                return;
            }

            // Step 7: Pass Allowed Requests Forward
            // If the limiter allows the request,
            // it’s forwarded to the next middleware or API controller as normal.
            await _next(context);
        }

        // Determines which rate-limiting policy applies based on request path. 
        // This ensures different API categories can have different rate limits
        // Example:
        //   /api/products → ProductApiPolicy
        //   /api/orders → OrderApiPolicy
        //   /api/payments → PaymentApiPolicy
        private static RateLimitPolicy GetPolicyFromPath(string path)
        {
            if (path.Contains("/products", StringComparison.OrdinalIgnoreCase))
                return RateLimitPolicy.ProductApi;

            if (path.Contains("/orders", StringComparison.OrdinalIgnoreCase))
                return RateLimitPolicy.OrderApi;

            if (path.Contains("/payments", StringComparison.OrdinalIgnoreCase))
                return RateLimitPolicy.PaymentApi;

            // Default fallback for all other routes
            return RateLimitPolicy.Default;
        }

        // Resolves a unique identity key for the current requester.
        // Priority order:
        //  1️. Authenticated user → Extract UserId from JWT claims.
        //  2️. Anonymous user → Use client IP address (proxy-aware).
        private static string ResolveIdentity(HttpContext context)
        {
            // 1️. Try JWT-based user identification.
            // Looks for claims like NameIdentifier, sub, or userId.
            // If found → identity becomes user: { userId}.
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User?.FindFirst("sub")?.Value
                      ?? context.User?.FindFirst("userId")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
                return $"user:{userId}";

            // If no JWT token (anonymous user):
            // Uses IP address as identity key.
            // Checks the X-Forwarded-For header(important when using proxies or CDNs).
            // Example: ip: 203.91.44.88
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? context.Connection.RemoteIpAddress?.ToString()
                     ?? "unknown";

            return $"ip:{ip}";
        }
    }
}
