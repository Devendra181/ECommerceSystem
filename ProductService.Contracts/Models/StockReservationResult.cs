using Messaging.Common.Models;
namespace ProductService.Contracts.Models
{
    // Represents the result of a stock reservation attempt.
    // This class is used to communicate back to the Orchestrator whether
    // the stock reservation was successful or failed — and why.
    public sealed class StockReservationResult
    {
        // Indicates whether the stock reservation succeeded for all requested items.
        // If true → all items were available and reserved successfully.
        // If false → one or more items could not be reserved.
        public bool Success { get; set; }

        // Provides a high-level reason for failure (e.g., "Insufficient stock").
        // This is mainly for logging, debugging, or customer-facing notifications.
        // Will be null or empty when Success = true.
        public string? FailureReason { get; set; }

        // A detailed list of items that failed to reserve.
        // Each entry specifies which product failed, how many were requested,
        // how many were available, and the failure reason.
        // Empty when all items succeed.
        public List<FailedLineItem> FailedItems { get; set; } = new();
    }
}
