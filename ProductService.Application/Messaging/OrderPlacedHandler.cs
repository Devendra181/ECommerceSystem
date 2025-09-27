using Messaging.Common.Events;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Contracts.Messaging;

namespace ProductService.Application.Messaging
{
    public class OrderPlacedHandler : IOrderPlacedHandler
    {
        private readonly IInventoryService _inventory;  // Dependency: Inventory service used to update product stock.

        // Constructor: injects IInventoryService via Dependency Injection.
        // This allows OrderPlacedHandler to call inventory logic without being tightly coupled.
        public OrderPlacedHandler(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        // HandleAsync: This method is triggered whenever an OrderPlacedEvent is received from RabbitMQ.
        public async Task HandleAsync(OrderPlacedEvent evt)
        {
            // Map event items into DTOs expected by the InventoryService.
            // Each order item (product + quantity) becomes an InventoryUpdateDTO.
            var stockUpdates = evt.Items.Select(i => new InventoryUpdateDTO
            {
                ProductId = i.ProductId,   // Product to update
                Quantity = i.Quantity      // Quantity to reduce
            }).ToList();

            // Call the inventory service to decrease stock for all products in bulk.
            // This ensures product quantities are reduced in the database after the order is confirmed.
            await _inventory.DecreaseStockBulkAsync(stockUpdates);
        }
    }
}

