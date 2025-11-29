using Microsoft.Extensions.Caching.Distributed;
using Serilog;
namespace APIGateway.Middlewares
{
    // Custom middleware responsible for handling response caching
    // at the API Gateway level using Redis Distributed Cache.
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;

        public ResponseCachingMiddleware(RequestDelegate next, IDistributedCache cache, IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // STEP 1: Check if caching is applicable for this request
            // Caching applies only to GET requests and only when globally enabled in appsettings.json.
            if (context.Request.Method != HttpMethods.Get ||
                !_config.GetValue<bool>("RedisCacheSettings:Enabled"))
            {
                // Move to the next middleware if conditions not met (no caching for POST/PUT/DELETE)
                await _next(context);
                return;
            }

            // STEP 2: Load per-endpoint cache configuration
            // CachePolicies defines which endpoints are cacheable and their TTL (in seconds).
            var cachePolicies = _config
                .GetSection("RedisCacheSettings:CachePolicies")
                .Get<Dictionary<string, int>>() ?? new();

            // Convert current request path to lowercase for case-insensitive matching
            var requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // STEP 3: Identify if current endpoint is explicitly configured for caching
            // We use StartsWith() to allow prefix matches (e.g., /products/products/123 → /products/products/)
            var matchedPolicy = cachePolicies
                .FirstOrDefault(p => requestPath.StartsWith(p.Key.ToLowerInvariant()));

            if (matchedPolicy.Key == null)
            {
                // If the route isn’t listed in CachePolicies → skip caching logic completely
                await _next(context);
                return;
            }

            // STEP 4: Determine the cache duration (TTL)
            // If a TTL is defined in CachePolicies, use it. Otherwise, fall back to DefaultCacheDurationInSeconds.
            var cacheDuration = matchedPolicy.Value > 0
                ? matchedPolicy.Value
                : _config.GetValue<int>("RedisCacheSettings:DefaultCacheDurationInSeconds");

            // STEP 5: Generate a unique and deterministic cache key
            // Key format → METHOD:/route/path?sortedQueryParameters
            var cacheKey = GenerateCacheKey(context);

            // STEP 6: Attempt to fetch the cached response from Redis
            string? cachedResponse = null;
            try 
            {
                 cachedResponse = await _cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {
                // If Redis is down or any cache operation fails, skip caching and
                // continue the pipeline so the request still succeeds instead of 500.
                Log.Error("Failed to get data from redis server: " + ex.ToString());
                await _next(context);
                return;
            }

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                // If found → directly return cached content to client (bypass microservice call)
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            // STEP 7: Capture the downstream (microservice) response for caching
            // Temporarily replace the response stream to intercept output
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Call the next middleware (which will eventually invoke the microservice)
            await _next(context);

            // STEP 8: Cache the response only if the request succeeded (HTTP 200 OK)
            if (context.Response.StatusCode == StatusCodes.Status200OK)
            {
                // Read response content from the memory stream
                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                // Store response in Redis with the computed TTL
                // Attempt to store response in Redis with the computed TTL. If Redis is
                // unavailable, swallow the exception but still return the response to client.
                try
                {
                    await _cache.SetStringAsync(cacheKey, responseBody, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheDuration)
                    });
                }
                catch
                {
                    // Ignore cache set failures to avoid converting them into 500 errors.
                    Log.Error("Failed to cache response for key {CacheKey}", cacheKey);
                }

                // Reset the stream and copy response back to the original output stream
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }

            // STEP 9: Restore the original response stream to continue pipeline execution
            context.Response.Body = originalBodyStream;
        }

        // Generates a normalized cache key based on HTTP method, path, and sorted query parameters.
        // Example:
        //   Request:  GET /products/products?pageSize=20&pageNumber=1
        //   CacheKey: GET:/products/products?pagenumber=1&pagesize=20
        private static string GenerateCacheKey(HttpContext context)
        {
            var request = context.Request;
            var method = request.Method.ToUpperInvariant();
            var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Sort and encode query parameters to ensure consistent key generation
            var query = request.Query
                .OrderBy(q => q.Key)
                .Select(q =>
                    $"{Uri.EscapeDataString(q.Key.ToLowerInvariant())}={Uri.EscapeDataString(q.Value)}");

            var queryString = string.Join("&", query);

            // Include query string only if present (avoid trailing '?')
            return string.IsNullOrEmpty(queryString)
                ? $"{method}:{path}"
                : $"{method}:{path}?{queryString}";
        }
    }
}
