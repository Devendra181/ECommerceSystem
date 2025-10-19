namespace Messaging.Common.Models
{
    // Represents details about a specific product line that failed during
    // the stock reservation process in the Product Microservice.

    // Purpose:
    //   - Helps identify which products caused the stock reservation failure.
    //   - Provides item-level diagnostics for OrchestratorService and NotificationService.
    //   - Enables clear audit trails and customer communication (e.g., “Item X is out of stock”).

    // Used In:
    //   - StockReservationFailedEvent (raised by ProductService)
    //   - OrderCancelledEvent (raised by OrchestratorService)

    // This model gives granular information on which item failed, how much stock was available,
    // and why it could not be reserved.
    public sealed class FailedLineItem
    {
        // ---------------------------------------------------------------------
        // Product Identifier
        // ---------------------------------------------------------------------
        // The unique ID of the product that failed during the reservation process.
        // Corresponds to ProductId in the ProductService database.

        // Used for:
        //   - Logging which product caused the issue.
        //   - Triggering compensating actions (like refunding or adjusting order status).
        public Guid ProductId { get; set; }

        // ---------------------------------------------------------------------
        // Requested Quantity
        // ---------------------------------------------------------------------
        // The number of units that the order originally requested for this product.

        // Example:
        //   If the customer ordered 5 units but only 2 were in stock,
        //   Requested = 5 and Available = 2.

        // Used for:
        //   - Comparing against actual available stock.
        //   - Determining whether the failure is full or partial.
        public int Requested { get; set; }

        // ---------------------------------------------------------------------
        // Available Quantity
        // ---------------------------------------------------------------------
        // The number of units that were actually available in stock at reservation time.

        // If 0 → completely out of stock.
        // If less than Requested → partial shortage.

        // Helps the Orchestrator decide whether to:
        //   - Cancel the entire order (if multiple failures).
        //   - Proceed partially (if some items are still available).
        public int Available { get; set; }

        // ---------------------------------------------------------------------
        // Failure Reason
        // ---------------------------------------------------------------------
        // Describes why this particular line item failed to reserve.
        // Default = "Insufficient stock", but it can also be customized
        // to represent other failure causes, such as:
        //   - "Product discontinued"
        //   - "Warehouse not reachable"
        //   - "Inventory sync error"
        // This field is especially useful for audit logs and customer-facing messages.
        public string Reason { get; set; } = "Insufficient stock";
    }
}
