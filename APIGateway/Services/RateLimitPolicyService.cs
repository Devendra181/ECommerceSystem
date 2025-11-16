using APIGateway.Helpers;
using APIGateway.Models;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace APIGateway.Services
{
    // Responsible for creating and configuring rate limiter instances 
    // based on policy definitions found in appsettings.json.

    // Design Goals:
    //  • Centralize all limiter creation logic in one place.
    //  • Support multiple limiter strategies (FixedWindow, ConcurrencyLimiter, etc.).
    //  • Allow global enable/disable toggle via configuration.
    public class RateLimitPolicyService : IRateLimitPolicyService
    {
        // Holds the strongly typed configuration from appsettings.json ("RateLimiting" section).
        private readonly IOptionsMonitor<RateLimitSettings> _rateLimitSettings;

        // Initializes the policy service using the options pattern to bind configuration.
        // The IOptions<T> abstraction automatically injects RateLimitSettings values from appsettings.json.
        public RateLimitPolicyService(IOptionsMonitor<RateLimitSettings> options)
        {
            _rateLimitSettings = options;
        }

        // Factory method for creating the appropriate RateLimiter based on the selected policy.
        public RateLimiter CreateRateLimiter(RateLimitPolicy policy)
        {
            var settings = _rateLimitSettings.CurrentValue;
            // If rate limiting is globally disabled (via appsettings.json),
            // return a lightweight NoopRateLimiter that allows all requests.
            if (!settings.IsEnabled)
                return new NoRateLimiter();

            // Dynamically choose limiter type based on policy
            return policy switch
            {
                // Product APIs → Use Fixed Window Limiter (e.g., 100 requests per minute)
                RateLimitPolicy.ProductApi => CreateFixedWindowLimiter(settings.ProductApiPolicy),

                // Order APIs → Also Fixed Window, typically stricter limits than Product
                RateLimitPolicy.OrderApi => CreateFixedWindowLimiter(settings.OrderApiPolicy),

                // Payment APIs → Use ConcurrencyLimiter (limits simultaneous requests)
                RateLimitPolicy.PaymentApi => CreateConcurrencyLimiter(settings.PaymentApiPolicy),

                // Fallback to Default Policy for all other APIs
                _ => CreateFixedWindowLimiter(settings.DefaultPolicy)
            };
        }

        // Creates a FixedWindowRateLimiter based on configuration values.
        // Fixed Window Limiter:
        // Allows up to N requests within a defined "window" duration (e.g., 60/sec, 100/min).
        // Once the window is full, requests are queued or rejected.
        private RateLimiter CreateFixedWindowLimiter(RateLimitSettings.Policy policy)
        {
            return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                // Maximum number of allowed requests in a window (PermitLimit = 100 → 100 req/min)
                PermitLimit = policy.PermitLimit,

                // The time duration defining each window (e.g., "00:01:00" = 1 minute)
                Window = TimeSpan.Parse(policy.Window),

                // Number of extra requests that can be queued after reaching the limit
                QueueLimit = policy.QueueLimit,

                // Defines how queued requests are processed (OldestFirst or NewestFirst)
                QueueProcessingOrder = Enum.Parse<QueueProcessingOrder>(policy.QueueProcessingOrder)
            });
        }

        // Creates a ConcurrencyLimiter based on configuration values.
        // Concurrency Limiter:
        // Restricts the number of concurrent (simultaneous) operations instead of per-minute requests. 
        // Ideal for Payment APIs, where only a few payment transactions 
        // should be processed at the same time to avoid gateway overload or double submissions.
        private RateLimiter CreateConcurrencyLimiter(RateLimitSettings.Policy policy)
        {
            return new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                // Number of concurrent operations permitted
                PermitLimit = policy.PermitLimit,

                // Additional requests waiting in queue (optional)
                QueueLimit = policy.QueueLimit,

                // Defines how queued requests are processed when slots free up
                QueueProcessingOrder = Enum.Parse<QueueProcessingOrder>(policy.QueueProcessingOrder)
            });
        }
    }
}
