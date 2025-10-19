using Messaging.Common.Events;
using Messaging.Common.Models;
using ProductService.Contracts.Messaging;
using ProductService.Contracts.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Messaging
{
    // Implements the core business logic for stock reservation within ProductService.
    //    This service is called when OrchestratorService publishes a
    //    StockReservationRequestedEvent (asking ProductService to check and hold stock).
    // 
    //    It checks inventory availability, updates stock if possible,
    //    and returns a StockReservationResult that indicates success or failure.
    public class StockReserveService : IStockReserveService
    {
        // Repository dependency that interacts with the database (Products table)
        private readonly IInventoryRepository _repo;

        public StockReserveService(IInventoryRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        // Validates and reserves stock for all products included in the given order.
        // 
        // Steps:
        //      1. Validate the request.
        //      2. Fetch product data from DB.
        //      3. Check available quantity vs requested quantity.
        //      4. If enough → decrement stock.
        //      5. If not → return a detailed failure result.
        public async Task<StockReservationResult> StockReserveAsync(StockReservationRequestedEvent request)
        {
            // Step 1: Input Validation
            // Ensure the request contains at least one item.
            if (request.Items == null || request.Items.Count == 0)
            {
                return new StockReservationResult
                {
                    Success = false,
                    FailureReason = "No items provided." // No items to process — invalid request.
                };
            }

            // Step 2: Fetch all requested products in one DB query
            // Extract distinct product IDs to minimize DB calls.
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

            // Get all products from DB for validation.
            var products = await _repo.GetProductsByIdsAsync(productIds);

            // SIMULATION STEP: Force a Negative Scenario
            // Example: Pretend that stock is lower than requested, even if it's not in DB.
            // Please uncomment the following to test the cancelled flow
            //foreach (var p in products)
            //{
            //    // Artificially set stock to a smaller number to trigger failure.
            //    p.StockQuantity = 1; // force insufficient stock
            //}

            // Create a dictionary for lookups by ProductId.
            var byId = products.ToDictionary(p => p.Id, p => p);

            // Step 3: Validate product existence and stock availability
            var failed = new List<FailedLineItem>();

            foreach (var line in request.Items)
            {
                // Case 1: Product does not exist in DB
                if (!byId.TryGetValue(line.ProductId, out var product))
                {
                    failed.Add(new FailedLineItem
                    {
                        ProductId = line.ProductId,
                        Requested = line.Quantity,
                        Available = 0,
                        Reason = "Product not found"
                    });
                    continue;
                }

                // Case 2: Product exists, but available quantity is less than requested
                var available = product.StockQuantity;
                if (available < line.Quantity)
                {
                    failed.Add(new FailedLineItem
                    {
                        ProductId = line.ProductId,
                        Requested = line.Quantity,
                        Available = available,
                        Reason = "Insufficient stock"
                    });
                }
            }

            // Step 4: If any validation failures occurred, return failure result
            if (failed.Count > 0)
            {
                return new StockReservationResult
                {
                    Success = false,
                    FailureReason = "Insufficient stock", // Common reason for all failed items
                    FailedItems = failed                   // Detailed per-product failure info
                };
            }

            // Step 5: Perform actual stock deduction for all items
            // Prepare a list of tuples like (ProductId, QuantityToReduce)
            var decrements = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();

            try
            {
                // Decrease stock for all items in one transaction.
                await _repo.DecreaseStockBulkAsync(decrements);

                // Return success response if DB update succeeds.
                return new StockReservationResult { Success = true };
            }
            catch (Exception ex)
            {
                // Handle any unexpected DB or concurrency errors.
                return new StockReservationResult
                {
                    Success = false,
                    FailureReason = ex.Message // Return the actual error for logging
                };
            }
        }
    }
}
