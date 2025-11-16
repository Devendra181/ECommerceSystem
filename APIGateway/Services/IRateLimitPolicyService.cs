using APIGateway.Models;
using System.Threading.RateLimiting;

namespace APIGateway.Services
{
    // Defines a contract for creating rate limiters dynamically 
    // based on API category (policy) and request identity.

    // The implementing service (RateLimitPolicyService) uses this interface
    // to generate the appropriate RateLimiter instance (FixedWindow, ConcurrencyLimiter, etc.)
    // depending on the policy selected by the middleware.
    public interface IRateLimitPolicyService
    {
        RateLimiter CreateRateLimiter(RateLimitPolicy policy);
    }
}
