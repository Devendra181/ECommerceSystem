namespace APIGateway.Models
{
    // Represents the global rate limiting configuration.
    // This model maps directly to the "RateLimiting" section in appsettings.json.
    public class RateLimitSettings
    {
        // Enables or disables rate limiting globally.
        // When false, the middleware bypasses all limiter checks (uses NoopRateLimiter).
        public bool IsEnabled { get; set; }

        // Default global policy applied to all endpoints
        // that do not have a specific API policy defined.
        public Policy DefaultPolicy { get; set; } = new();

        // Rate limiting rules specifically for Product-related APIs.
        // Example: /api/products/*
        public Policy ProductApiPolicy { get; set; } = new();

        // Rate limiting rules specifically for Order-related APIs.
        // Example: /api/orders/*
        public Policy OrderApiPolicy { get; set; } = new();

        // Rate limiting rules specifically for Payment-related APIs.
        // Example: /api/payments/*
        // Typically configured with a stricter limit
        // since payment operations are sensitive and require throttling.
        public Policy PaymentApiPolicy { get; set; } = new();

        // Inner class representing an individual rate limiting policy.
        // Each policy defines how many requests can be made in a specific time window
        // and how excess requests are queued or rejected.
        public class Policy
        {
            // Maximum number of requests permitted within a given time window.
            // Example: PermitLimit = 100 means 100 requests per window per user/IP.
            public int PermitLimit { get; set; }

            // The duration of the fixed or sliding window.
            // Format: "hh:mm:ss" (e.g., "00:01:00" = 1 minute window).
            public string Window { get; set; } = "00:01:00";

            // The number of requests that can wait in the queue
            public int QueueLimit { get; set; }

            // The order in which queued requests are processed:
            //  "OldestFirst" → FIFO (first-come, first-served)
            //  "NewestFirst" → LIFO (newest request gets priority)
            // This maps to the QueueProcessingOrder enum in System.Threading.RateLimiting.
            public string QueueProcessingOrder { get; set; } = "OldestFirst";
        }
    }
}

