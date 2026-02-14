using AutoMapper;
using MediatR;
using Messaging.Common.Events;
using Messaging.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Orders.Commands;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Contracts.ExternalServices;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Handlers.Commands
{
    // CQRS Command Handler:
    // Handles the CreateOrderCommand and returns an OrderResponseDTO.

    // Business meaning:
    // - This is the "WRITE" side operation that:
    //   1) Validates user and addresses via User Microservice.
    //   2) Validates product stock via Product Microservice.
    //   3) Builds the Order aggregate with items, policies, and pricing.
    //   4) Calculates discount, tax, shipping, and total.
    //   5) Persists the Order in the database.
    //   6) Initiates payment via Payment Microservice.
    //   7) For COD:
    //        * Immediately publishes an OrderPlacedEvent to RabbitMQ so:
    //            - ProductService can reserve/reduce stock.
    //            - NotificationService can send order-confirmation messages.
    //      For Online payment:
    //        * Returns payment URL and keeps order in Pending state.
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDTO>
    {
        // Repository for reading/writing orders to the database.
        private readonly IOrderRepository _orderRepository;

        // User Microservice client:
        // - Validate that user exists.
        // - Manage addresses (save/update).
        private readonly IUserServiceClient _userServiceClient;

        // Product Microservice client:
        // - Validate product stock.
        // - Fetch latest product details (price, discount, etc.).
        private readonly IProductServiceClient _productServiceClient;

        // Payment Microservice client:
        // - Initiate payment for NON-COD orders.
        private readonly IPaymentServiceClient _paymentServiceClient;

        // Master data repository:
        // - Fetch cancellation and return policies.
        // - Fetch discounts and tax rules.
        private readonly IMasterDataRepository _masterDataRepository;

        // AutoMapper:
        // - Map Order entity -> OrderResponseDTO.
        private readonly IMapper _mapper;

        // Configuration:
        // - Used for shipping configuration (free threshold, charges, etc.).
        private readonly IConfiguration _configuration;

        // Publisher for OrderPlacedEvent:
        // - Sends integration events to RabbitMQ.
        private readonly IOrderPlacedEventPublisher _publisher;

        // Logger:
        // - Used to log errors and diagnostics.
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        // Constructor:
        // All dependencies are injected via DI.

        // Note: notificationServiceClient is injected but not stored in a field.
        //       You can later use it directly for synchronous notifications if needed.
        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUserServiceClient userServiceClient,
            IProductServiceClient productServiceClient,
            IPaymentServiceClient paymentServiceClient,
            IMasterDataRepository masterDataRepository,
            IMapper mapper,
            IConfiguration configuration,
            IOrderPlacedEventPublisher publisher,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _userServiceClient = userServiceClient ?? throw new ArgumentNullException(nameof(userServiceClient));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
            _masterDataRepository = masterDataRepository ?? throw new ArgumentNullException(nameof(masterDataRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Handle method:
        // This is the core logic for creating an order.
        // High-level flow:
        // 1) Validate basic request (non-null, has items).
        // 2) Validate user existence in UserService.
        // 3) Resolve shipping/billing address IDs (existing or newly created).
        // 4) Validate product availability (stock check).
        // 5) Fetch latest product details.
        // 6) Build the Order + items (with policies).
        // 7) Calculate discounts, tax, shipping, and total.
        // 8) Persist the order via repository.
        // 9) Initiate payment via PaymentService.
        // 10) For COD:
        //      - Publish OrderPlacedEvent immediately.
        //      - Return Confirmed order DTO.
        //     For Online payment:
        //      - Return Pending order DTO + PaymentUrl.
        public async Task<OrderResponseDTO> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            // Extract the data from the command.
            var request = command.Request;
            var accessToken = command.AccessToken;

            // 1. Basic validation of the incoming request.
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Ensure there is at least one order item.
            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("Order must have at least one item.");

            // 2. Validate that the user exists in the User Microservice.
            //    - If user doesn't exist, we cannot proceed with the order.
            var user = await _userServiceClient.GetUserByIdAsync(request.UserId, accessToken);
            if (user == null)
                throw new InvalidOperationException("User does not exist.");

            // 3. Resolve Shipping Address ID:
            //    - If ShippingAddressId is provided, use it.
            //    - Otherwise, if ShippingAddress DTO is provided, save it via UserService and get its ID.
            //    - This ensures we always have a valid ShippingAddressId to store in the Order.
            Guid? shippingAddressId = null;
            if (request.ShippingAddressId != null)
            {
                // Use existing shipping address ID.
                shippingAddressId = request.ShippingAddressId;
            }
            else if (request.ShippingAddress != null)
            {
                // Create/update shipping address in User Microservice and use the returned ID.
                request.ShippingAddress.UserId = request.UserId;
                shippingAddressId = await _userServiceClient.SaveOrUpdateAddressAsync(request.ShippingAddress, accessToken);
            }

            // 4. Resolve Billing Address ID:
            //    - Same logic as shipping address (existing ID or create new).
            Guid? billingAddressId = null;
            if (request.BillingAddressId != null)
            {
                billingAddressId = request.BillingAddressId;
            }
            else if (request.BillingAddress != null)
            {
                request.BillingAddress.UserId = request.UserId;
                billingAddressId = await _userServiceClient.SaveOrUpdateAddressAsync(request.BillingAddress, accessToken);
            }

            // Ensure both addresses are present at this point.
            if (shippingAddressId == null || billingAddressId == null)
                throw new ArgumentException("Both ShippingAddressId and BillingAddressId must be provided or created.");

            // 5. Validate product stock availability but DO NOT reduce stock yet.
            //    - This is just a pre-check; actual stock reduction will be done
            //      by ProductService when it consumes the OrderPlacedEvent.
            var stockCheckRequests = request.Items.Select(i => new ProductStockVerificationRequestDTO
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();

            var stockValidation = await _productServiceClient.CheckProductsAvailabilityAsync(stockCheckRequests, accessToken);
            if (stockValidation == null || stockValidation.Any(x => !x.IsValidProduct || !x.IsQuantityAvailable))
                throw new InvalidOperationException("One or more products are invalid or out of stock.");

            // 6. Retrieve the latest product info for accurate pricing/discount.
            //    - This ensures we don't rely on stale price data sent from the client.
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _productServiceClient.GetProductsByIdsAsync(productIds, accessToken);
            if (products == null || products.Count != productIds.Count)
                throw new InvalidOperationException("Failed to retrieve product details for all items.");

            try
            {
                // 7. Fetch policy data from MasterData (if applicable).
                int? cancellationPolicyId = null;
                int? returnPolicyId = null;

                var cancellationPolicy = await _masterDataRepository.GetActiveCancellationPolicyAsync();
                if (cancellationPolicy != null)
                    cancellationPolicyId = cancellationPolicy.Id;

                var returnPolicy = await _masterDataRepository.GetActiveReturnPolicyAsync();
                if (returnPolicy != null)
                    returnPolicyId = returnPolicy.Id;

                // 8. Prepare order identity and timestamps.
                var orderId = Guid.NewGuid();
                var orderNumber = GenerateOrderNumberFromGuid(orderId); // Human-friendly readable ID.
                var now = DateTime.UtcNow;

                // Determine initial order status based on payment method:
                // - COD:
                //    * We treat it as Confirmed immediately.
                //    * Stock will be reduced after event is processed.
                // - Online:
                //    * Start as Pending until payment is completed.
                var initialStatus = request.PaymentMethod == PaymentMethodEnum.COD
                    ? OrderStatusEnum.Confirmed
                    : OrderStatusEnum.Pending;

                // 9. Create the Order entity (aggregate root).
                var order = new Order
                {
                    Id = orderId,
                    OrderNumber = orderNumber,
                    UserId = request.UserId,
                    ShippingAddressId = shippingAddressId.Value,
                    BillingAddressId = billingAddressId.Value,
                    PaymentMethod = request.PaymentMethod.ToString(),
                    OrderStatusId = (int)initialStatus,
                    CreatedAt = now,
                    OrderDate = now,
                    CancellationPolicyId = cancellationPolicyId,
                    ReturnPolicyId = returnPolicyId,
                    OrderItems = new List<OrderItem>()
                };

                // 10. Add OrderItems using fresh product data from ProductService.
                foreach (var item in request.Items)
                {
                    // Find matching product details.
                    var product = products.First(p => p.Id == item.ProductId);

                    order.OrderItems.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        PriceAtPurchase = product.Price,
                        DiscountedPrice = product.DiscountedPrice,
                        Quantity = item.Quantity,
                        ItemStatusId = (int)initialStatus
                    });
                }

                // 11. Calculate pricing (Subtotal, Discount, Tax, Shipping, Total).
                //     - SubTotalAmount: Sum of price * quantity, before discounts.
                order.SubTotalAmount = Math.Round(
                    order.OrderItems.Sum(i => i.PriceAtPurchase * i.Quantity),
                    2,
                    MidpointRounding.AwayFromZero);

                //     - DiscountAmount: Product-level discounts + best order-level discount.
                order.DiscountAmount = Math.Round(
                    await CalculateDiscountAmountAsync(order.OrderItems),
                    2,
                    MidpointRounding.AwayFromZero);

                //     - TaxAmount: Taxes applied on (Subtotal - Discount).
                order.TaxAmount = Math.Round(
                    await CalculateTaxAmountAsync(order.SubTotalAmount - order.DiscountAmount),
                    2,
                    MidpointRounding.AwayFromZero);

                //     - ShippingCharges: Based on total after discount and config rules.
                order.ShippingCharges = Math.Round(
                    CalculateShippingCharges(order.SubTotalAmount - order.DiscountAmount),
                    2,
                    MidpointRounding.AwayFromZero);

                //     - TotalAmount: Final amount the customer has to pay.
                order.TotalAmount = Math.Round(
                    order.SubTotalAmount - order.DiscountAmount + order.TaxAmount + order.ShippingCharges,
                    2,
                    MidpointRounding.AwayFromZero);

                // 12. Persist the Order in the database.
                var addedOrder = await _orderRepository.AddAsync(order);
                if (addedOrder == null)
                    throw new InvalidOperationException("Failed to create order.");

                // 13. Initiate payment via Payment Microservice.
                //     - Even for COD, you might still record a "zero" or "COD" payment entry.
                var paymentRequest = new CreatePaymentRequestDTO
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Amount = order.TotalAmount,
                    PaymentMethod = request.PaymentMethod
                };

                var paymentResponse = await _paymentServiceClient.InitiatePaymentAsync(paymentRequest, accessToken);
                if (paymentResponse == null)
                    throw new InvalidOperationException("Payment initiation failed.");

                // 14. Behavior differs based on payment method:

                // Case A: COD (Cash on Delivery)
                // - Order is already marked as Confirmed.
                // - We immediately publish OrderPlacedEvent so ProductService and NotificationService can act.
                if (request.PaymentMethod == PaymentMethodEnum.COD)
                {
                    #region Event Publishing to RabbitMQ

                    var orderPlacedEvent = new OrderPlacedEvent
                    {
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        UserId = order.UserId,
                        CustomerName = user.FullName,
                        CustomerEmail = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        TotalAmount = order.TotalAmount,
                        CorrelationId = order.Id.ToString(),
                        Items = order.OrderItems.Select(i => new OrderLineItem
                        {
                            ProductId = i.ProductId,
                            //Name = i.ProductName,
                            Quantity = i.Quantity,
                            UnitPrice = i.PriceAtPurchase
                        }).ToList()
                    };

                    // Publish the integration event to RabbitMQ using the shared publisher.
                    // This message will be routed to:
                    //    - ProductService (to decrease stock)
                    //    - NotificationService (to insert notification record).
                    //    - Use OrderId as CorrelationId for traceability.
                    await _publisher.PublishOrderPlacedAsync(orderPlacedEvent);

                    #endregion

                    // Map Order -> OrderResponseDTO and adjust status/payment info for COD.
                    var orderDto = _mapper.Map<OrderResponseDTO>(order);
                    orderDto.OrderStatus = OrderStatusEnum.Confirmed;
                    orderDto.PaymentMethod = PaymentMethodEnum.COD;
                    orderDto.PaymentUrl = null; // No payment URL for COD.

                    return orderDto;
                }
                else
                {
                    // Case B: Online Payment
                    // - Order is in Pending state.
                    // - We return the payment URL so the client can redirect the user to payment page.
                    var orderDto = _mapper.Map<OrderResponseDTO>(order);
                    orderDto.OrderStatus = OrderStatusEnum.Pending;
                    orderDto.PaymentMethod = request.PaymentMethod;
                    orderDto.PaymentUrl = paymentResponse.PaymentUrl;

                    return orderDto;
                }
            }
            catch (Exception ex)
            {
                // Log the error with user context (UserId) for easier debugging.
                _logger.LogError(ex, "Error while creating order for user {UserId}", command.Request.UserId);

                // Re-throw the exception so upper layers (API, orchestrator, etc.) can handle it.
                throw;
            }
        }

        #region Private Helpers (moved from OrderService)

        // Calculates total discount: product-level + best order-level discount.

        // Product-level:
        // - Based on difference between PriceAtPurchase and DiscountedPrice * Quantity.

        // Order-level:
        // - Based on active discounts configured in master data.
        // - Applies the single "best" discount (highest percentage or amount).
        private async Task<decimal> CalculateDiscountAmountAsync(IEnumerable<OrderItem> orderItems)
        {
            decimal productLevelDiscountTotal = 0m;

            // Sum discounts applied on individual products (multiplied by quantity).
            foreach (var item in orderItems)
            {
                productLevelDiscountTotal += (item.PriceAtPurchase - item.DiscountedPrice) * item.Quantity;
            }

            decimal orderLevelDiscount = 0m;
            DateTime today = DateTime.UtcNow.Date;

            // Retrieve currently active order-level discounts.
            var activeOrderDiscounts = await _masterDataRepository.GetActiveDiscountsAsync(today);

            // Select the best discount (simple rule: highest effective value).
            var bestOrderDiscount = activeOrderDiscounts
                .Where(d => d.IsActive && d.StartDate <= today && d.EndDate >= today)
                .OrderByDescending(d => d.DiscountType == DiscountTypeEnum.Percentage
                    ? d.DiscountPercentage ?? 0
                    : d.DiscountAmount ?? 0)
                .FirstOrDefault();

            if (bestOrderDiscount != null)
            {
                // Compute order subtotal AFTER product-level discounts (using DiscountedPrice).
                decimal orderSubtotal = orderItems.Sum(i => i.DiscountedPrice * i.Quantity);

                if (bestOrderDiscount.DiscountType == DiscountTypeEnum.Percentage &&
                    bestOrderDiscount.DiscountPercentage.HasValue)
                {
                    // Percentage-based discount on orderSubtotal.
                    orderLevelDiscount = orderSubtotal * (bestOrderDiscount.DiscountPercentage.Value / 100m);
                }
                else if (bestOrderDiscount.DiscountType == DiscountTypeEnum.FixedAmount &&
                         bestOrderDiscount.DiscountAmount.HasValue)
                {
                    // Fixed-amount discount (e.g., ₹200 off).
                    orderLevelDiscount = bestOrderDiscount.DiscountAmount.Value;
                }
            }

            // Final discount = product-level discounts + best order-level discount.
            return productLevelDiscountTotal + orderLevelDiscount;
        }

        // Calculates total tax based on active tax rules and the taxable amount.

        // TaxableAmount:
        // - Typically (Subtotal - Discount).

        // Tax rules:
        // - Fetched from master data.
        // - Only active taxes within valid date range are applied.
        private async Task<decimal> CalculateTaxAmountAsync(decimal taxableAmount)
        {
            decimal totalTax = 0m;
            DateTime today = DateTime.UtcNow.Date;

            var activeTaxes = await _masterDataRepository.GetActiveTaxesAsync(today);

            foreach (var tax in activeTaxes)
            {
                // Apply only active taxes within valid date range.
                if (tax.IsActive &&
                    (!tax.ValidTo.HasValue || tax.ValidTo >= today) &&
                    tax.ValidFrom <= today)
                {
                    // In this example, we apply tax only to products.
                    if (tax.AppliesToProduct)
                    {
                        totalTax += taxableAmount * (tax.TaxPercentage / 100m);
                    }
                }
            }

            return totalTax;
        }

        // Calculates shipping charges based on:
        // - Whether shipping charge is enabled.
        // - Free shipping threshold.
        // - Flat shipping charge from configuration.
        private decimal CalculateShippingCharges(decimal orderTotal)
        {
            bool isShippingAllowed = _configuration.GetValue<bool>("ShippingConfig:IsShippingChargeAllowed");
            decimal freeShippingThreshold = _configuration.GetValue<decimal>("ShippingConfig:FreeShippingThreshold");
            decimal shippingCharge = _configuration.GetValue<decimal>("ShippingConfig:ShippingCharge");

            // If shipping charges are disabled, always return 0.
            if (!isShippingAllowed)
                return 0m;

            // If the order total exceeds the free shipping threshold, no shipping charge.
            if (orderTotal >= freeShippingThreshold)
                return 0m;

            // Otherwise, apply the configured flat shipping charge.
            return shippingCharge;
        }

        // Generates a human-readable Order Number from a GUID.
        // Format example:
        //   ORD20260110ABCDE123
        // where:
        //   - "ORD"       : Prefix.
        //   - "20260110"  : Current date (yyyyMMdd).
        //   - "ABCDE123"  : Last 8 characters of GUID (uppercase).
        private string GenerateOrderNumberFromGuid(Guid orderId)
        {
            var prefix = "ORD";
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            // Get last 8 characters from GUID string (without dashes).
            var guidString = orderId.ToString("N"); // N = digits only, no dashes.
            var guidSuffix = guidString.Substring(guidString.Length - 8, 8).ToUpper();

            return $"{prefix}{datePart}{guidSuffix}";
        }

        #endregion
    }
}
