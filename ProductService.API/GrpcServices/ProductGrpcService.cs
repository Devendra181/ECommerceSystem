using ECommerce.GrpcContracts.Products;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using System.Globalization;

namespace ProductService.API.GrpcServices
{
    // ============================================================================
    // gRPC Server Implementation for ProductService
    // ----------------------------------------------------------------------------
    // This class implements the RPC operations defined in product.proto:
    //
    // service ProductGrpc {
    //   rpc GetProductsByIds(...)
    //   rpc CheckProductsAvailability(...)
    //   rpc IncreaseStockBulk(...)
    //   rpc DecreaseStockBulk(...)
    // }
    //
    // Notes:
    // - This is the gRPC equivalent of a REST Controller
    // - Business logic lives in Application layer (IProductService, IInventoryService)
    // - This class performs validation, mapping, logging, and error translation
    // ============================================================================
    public sealed class ProductGrpcService : ProductGrpc.ProductGrpcBase
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductGrpcService> _logger;

        // Constructor with dependency injection
        public ProductGrpcService(
            IProductService productService,
            IInventoryService inventoryService,
            ILogger<ProductGrpcService> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================================================
        // 1. GET PRODUCTS BY IDS
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Fetch product details in bulk
        //   - Used during order creation to calculate pricing and discounts
        //
        // Flow:
        //   (a) Validate product_ids
        //   (b) Convert string IDs → Guid
        //   (c) Call application service
        //   (d) Map domain entities → protobuf Product messages
        // ============================================================================
        public override async Task<GetProductsByIdsReply> GetProductsByIds(
            GetProductsByIdsRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation(
                "gRPC GetProductsByIds called. count={Count}",
                request.ProductIds?.Count ?? 0);

            // Validate request
            if (request.ProductIds == null || request.ProductIds.Count == 0)
            {
                _logger.LogWarning("GetProductsByIds failed: product_ids missing.");
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "product_ids required."));
            }

            // Convert product IDs from string → Guid
            var ids = request.ProductIds.Select(id =>
            {
                if (!Guid.TryParse(id, out var gid))
                {
                    _logger.LogWarning("Invalid product_id received: {ProductId}", id);
                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument, $"Invalid product_id: {id}"));
                }
                return gid;
            }).ToList();

            // Call application-layer service
            var products = await _productService.GetByIdsAsync(ids);

            _logger.LogInformation(
                "Products retrieved successfully. requested={Requested}, found={Found}",
                ids.Count, products.Count);

            // Map domain entities → protobuf response
            var reply = new GetProductsByIdsReply();
            reply.Products.AddRange(products.Select(p => new ECommerce.GrpcContracts.Products.Product
            {
                Id = p.Id.ToString(),
                Name = p.Name ?? string.Empty,

                // decimal → string to avoid precision loss
                Price = p.Price.ToString(CultureInfo.InvariantCulture),
                DiscountedPrice = p.DiscountedPrice.ToString(CultureInfo.InvariantCulture),

                StockQuantity = p.StockQuantity
            }));

            return reply;
        }

        // ============================================================================
        // 2. CHECK PRODUCTS AVAILABILITY
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Validate stock before order placement
        //   - Fail fast if product or quantity is invalid
        //
        // Flow:
        //   (a) Validate request items
        //   (b) Convert to ProductStockInfoRequestDTO
        //   (c) Call inventory service
        //   (d) Map results → protobuf response
        // ============================================================================
        public override async Task<CheckProductsAvailabilityReply> CheckProductsAvailability(
            CheckProductsAvailabilityRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation(
                "gRPC CheckProductsAvailability called. itemCount={Count}",
                request.Items?.Count ?? 0);

            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("CheckProductsAvailability failed: items missing.");
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "items required."));
            }

            // Convert protobuf items → application DTOs
            var dto = request.Items.Select(i =>
            {
                if (!Guid.TryParse(i.ProductId, out var pid))
                {
                    _logger.LogWarning(
                        "Invalid product_id in stock check: {ProductId}", i.ProductId);
                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument,
                                   $"Invalid product_id: {i.ProductId}"));
                }

                return new ProductStockInfoRequestDTO
                {
                    ProductId = pid,
                    Quantity = i.Quantity
                };
            }).ToList();

            // Call inventory service
            var results = await _inventoryService.VerifyStockForProductsAsync(dto);

            _logger.LogInformation(
                "Stock verification completed. itemCount={Count}",
                results.Count);

            // Map application results → protobuf response
            var reply = new CheckProductsAvailabilityReply();
            reply.Results.AddRange(results.Select(r => new StockCheckResult
            {
                ProductId = r.ProductId.ToString(),
                IsValidProduct = r.IsValidProduct,
                IsQuantityAvailable = r.IsQuantityAvailable,
                AvailableQuantity = r.AvailableQuantity
            }));

            return reply;
        }

        // ============================================================================
        // 3. INCREASE STOCK BULK
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Increase inventory in bulk
        //   - Used during order cancellation or returns
        // ============================================================================
        public override async Task<StockBulkUpdateReply> IncreaseStockBulk(
            StockBulkUpdateRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC IncreaseStockBulk called.");

            var updates = ToInventoryUpdates(request);

            await _inventoryService.IncreaseStockBulkAsync(updates);

            _logger.LogInformation(
                "Stock increased successfully. updateCount={Count}",
                updates.Count);

            return new StockBulkUpdateReply
            {
                Success = true,
                Message = "Stock increased."
            };
        }

        // ============================================================================
        // 4. DECREASE STOCK BULK
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Decrease inventory after successful order placement
        // ============================================================================
        public override async Task<StockBulkUpdateReply> DecreaseStockBulk(
            StockBulkUpdateRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC DecreaseStockBulk called.");

            var updates = ToInventoryUpdates(request);

            await _inventoryService.DecreaseStockBulkAsync(updates);

            _logger.LogInformation(
                "Stock decreased successfully. updateCount={Count}",
                updates.Count);

            return new StockBulkUpdateReply
            {
                Success = true,
                Message = "Stock decreased."
            };
        }

        // ============================================================================
        // HELPER: Convert StockBulkUpdateRequest → InventoryUpdateDTO
        // ----------------------------------------------------------------------------
        // Responsibility:
        //   - Validate incoming gRPC data
        //   - Convert string IDs → Guid
        //   - Return application-layer DTOs
        // ============================================================================
        private static List<InventoryUpdateDTO> ToInventoryUpdates(
            StockBulkUpdateRequest request)
        {
            if (request.Updates == null || request.Updates.Count == 0)
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "updates required."));

            return request.Updates.Select(u =>
            {
                if (!Guid.TryParse(u.ProductId, out var pid))
                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument,
                                   $"Invalid product_id: {u.ProductId}"));

                return new InventoryUpdateDTO
                {
                    ProductId = pid,
                    Quantity = u.Quantity
                };
            }).ToList();
        }
    }
}
