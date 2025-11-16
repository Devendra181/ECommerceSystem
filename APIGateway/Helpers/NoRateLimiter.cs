using System.Threading.RateLimiting;

namespace APIGateway.Helpers
{
    // Used when global rate limiting is turned off (IsEnabled = false).
    // Always allows requests without applying any throttling.
    public sealed class NoRateLimiter : RateLimiter
    {
        // Reusable "always-allowed" lease for all requests.
        private static readonly RateLimitLease _lease = new NoLease();

        // Indicates the limiter is always active (never idle).
        public override TimeSpan? IdleDuration => Timeout.InfiniteTimeSpan;

        // Synchronously grants permission for every request.
        protected override RateLimitLease AttemptAcquireCore(int permitCount)
        {
            return _lease;
        }

        // Asynchronous method that grants permission immediately for all requests.
        // Used when rate limiting is globally disabled.
        protected override ValueTask<RateLimitLease> AcquireAsyncCore(
            int permitCount,
            CancellationToken cancellationToken)
        {
            // Always returns a successful lease without any delay or throttling.
            return new ValueTask<RateLimitLease>(_lease);
        }

        // No statistics tracking for this limiter.
        public override RateLimiterStatistics? GetStatistics()
        {
            return null;
        }

        // Represents a successful lease that always allows requests.
        private sealed class NoLease : RateLimitLease
        {
            // Always grants permission.
            public override bool IsAcquired => true;

            // No metadata (e.g., retry info) is provided.
            public override IEnumerable<string> MetadataNames => Array.Empty<string>();

            // Always returns false since no metadata exists.
            public override bool TryGetMetadata(string name, out object? metadata)
            {
                metadata = null;
                return false;
            }
        }
    }
}
