using Messaging.Common.Events;
using ProductService.Contracts.Models;

namespace ProductService.Contracts.Messaging
{
    // Defines the contract for stock reservation logic within the ProductService. 
    public interface IStockReserveService
    {
        // Validates and reserves stock for the given order request.
        // This method is called when the OrchestratorService sends a 
        // StockReservationRequestedEvent asking the ProductService 
        // to verify and hold stock for all products in an order.
        // Business Flow:
        //      1. Check inventory for each product ID and requested quantity.
        //      2. If all items are available → deduct stock and mark Success = true.
        //      3. If any item is out of stock → mark Success = false and return failed items.
        //      4. The result (success or failure) is then published back to RabbitMQ.
        Task<StockReservationResult> StockReserveAsync(StockReservationRequestedEvent request);
    }
}

