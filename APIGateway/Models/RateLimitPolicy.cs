namespace APIGateway.Models
{
    // Each enum value corresponds to a specific configuration section 
    // inside the RateLimiting section of appsettings.json file.
    // Example (JSON):
    // "RateLimiting": {
    //   "DefaultPolicy": { ... },
    //   "ProductApiPolicy": { ... },
    //   "OrderApiPolicy": { ... },
    //   "PaymentApiPolicy": { ... }
    // }

    public enum RateLimitPolicy
    {
        Default,
        ProductApi,
        OrderApi,
        PaymentApi
    }
}
