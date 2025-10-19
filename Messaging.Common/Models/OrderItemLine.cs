namespace Messaging.Common.Models
{
    // Represents a single product line item within an order.

    // Purpose:
    //  - Standardize how order items are represented in all event messages.
    //  - Avoid duplication of similar models in different services.
    //  - Ensure consistency between OrderService, ProductService, and NotificationService.

    // Typical Usage:
    //  - Inside OrderPlacedEvent → carries all ordered products.
    //  - Inside StockReservationRequestedEvent → specifies which products need stock check.
    //  - Inside StockReservedCompletedEvent → confirms what was successfully reserved.
    //  - Inside OrderConfirmedEvent → included for final confirmation and email notifications.
    public sealed class OrderLineItem
    {
        // --------------------------------------------------------------------
        // Product Identity
        // --------------------------------------------------------------------
        // The unique identifier (GUID) of the product being ordered.
        // Used for:
        //   - Matching inventory in ProductService.
        //   - Calculating stock levels.
        //   - Linking order details to product catalog data.
        public Guid ProductId { get; set; }

        // --------------------------------------------------------------------
        // Quantity
        // --------------------------------------------------------------------
        // The total number of units of this product included in the order.
        // Used for:
        //   - Stock validation and reservation.
        //   - Calculating the total line price (Quantity × UnitPrice).
        //   - Generating invoices and order summaries.
        public int Quantity { get; set; }

        // --------------------------------------------------------------------
        // Pricing Information
        // --------------------------------------------------------------------
        // The price per unit of the product at the time of order placement.
        // Important Notes:
        //   - Captures the price snapshot at order time (even if price later changes).
        //   - Used for financial calculations in OrderService and for displaying
        //     correct totals in notifications and invoices.
        // Example:
        //   If Quantity = 3 and UnitPrice = 500, total line cost = 1500.
        public decimal UnitPrice { get; set; }// Price per unit at time of order
    }
}
