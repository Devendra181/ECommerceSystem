using AutoMapper;
using Messaging.Common.Events;
using Microsoft.Extensions.Configuration;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.Interfaces;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Contracts.ExternalServices;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IPaymentServiceClient _paymentServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly IMapper _mapper;
        private readonly IMasterDataRepository _masterDataRepository;
        private readonly IConfiguration _configuration;
        private readonly IOrderEventPublisher _publisher;

        public OrderService(
            IOrderRepository orderRepository,
            IUserServiceClient userServiceClient,
            IProductServiceClient productServiceClient,
            IPaymentServiceClient paymentServiceClient,
            INotificationServiceClient notificationServiceClient,
            IMasterDataRepository masterDataRepository,
            IMapper mapper,
            IConfiguration configuration,
            IOrderEventPublisher publisher)
        {
            // Initialize dependencies with null checks for safe injection
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _userServiceClient = userServiceClient ?? throw new ArgumentNullException(nameof(userServiceClient));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
            _notificationServiceClient = notificationServiceClient ?? throw new ArgumentNullException(nameof(notificationServiceClient));
            _masterDataRepository = masterDataRepository ?? throw new ArgumentNullException(nameof(masterDataRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _publisher = publisher;
        }

        // Create a new order including user validation, address handling, stock check,
        // pricing calculations, payment initiation, and transactional consistency.
        public async Task<OrderResponseDTO> CreateOrderAsync(CreateOrderRequestDTO request, string accessToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("Order must have at least one item.");

            // Validate that the user exists via User Microservice
            var user = await _userServiceClient.GetUserByIdAsync(request.UserId, accessToken);
            if (user == null)
                throw new InvalidOperationException("User does not exist.");

            // Resolve Shipping Address ID, either provided or created newly via User Microservice
            Guid? shippingAddressId = null;
            if (request.ShippingAddressId != null)
            {
                shippingAddressId = request.ShippingAddressId;
            }
            else if (request.ShippingAddress != null)
            {
                request.ShippingAddress.UserId = request.UserId;
                shippingAddressId = await _userServiceClient.SaveOrUpdateAddressAsync(request.ShippingAddress, accessToken);
            }

            // Resolve Billing Address ID, either provided or created newly
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

            // Validate presence of both addresses
            if (shippingAddressId == null || billingAddressId == null)
                throw new ArgumentException("Both ShippingAddressId and BillingAddressId must be provided or created.");

            // Validate product stock availability but do not reduce stock yet
            var stockCheckRequests = request.Items.Select(i => new ProductStockVerificationRequestDTO
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();

            var stockValidation = await _productServiceClient.CheckProductsAvailabilityAsync(stockCheckRequests, accessToken);
            if (stockValidation == null || stockValidation.Any(x => !x.IsValidProduct || !x.IsQuantityAvailable))
                throw new InvalidOperationException("One or more products are invalid or out of stock.");

            // Retrieve latest product info for accurate pricing and discount
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _productServiceClient.GetProductsByIdsAsync(productIds, accessToken);
            if (products == null || products.Count != productIds.Count)
                throw new InvalidOperationException("Failed to retrieve product details for all items.");

            try
            {

                // Fetch policies (example placeholders, adjust with your actual logic)
                int? cancellationPolicyId = null;
                int? returnPolicyId = null;

                // Example: fetch cancellation policy based on user or other criteria
                var cancellationPolicy = await _masterDataRepository.GetActiveCancellationPolicyAsync();
                if (cancellationPolicy != null)
                    cancellationPolicyId = cancellationPolicy.Id;

                var returnPolicy = await _masterDataRepository.GetActiveReturnPolicyAsync();
                if (returnPolicy != null)
                    returnPolicyId = returnPolicy.Id;

                var orderId = Guid.NewGuid();
                var orderNumber = GenerateOrderNumberFromGuid(orderId);
                var now = DateTime.UtcNow;

                var initialStatus = request.PaymentMethod == PaymentMethodEnum.COD
                    ? OrderStatusEnum.Confirmed   // COD orders confirmed immediately
                    : OrderStatusEnum.Pending;    // Online payment orders start as pending

                // Create order entity
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

                // Add order items with fresh product data
                foreach (var item in request.Items)
                {
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

                // Calculate order totals: subtotal, discount, tax, shipping, and final amount
                order.SubTotalAmount = Math.Round(order.OrderItems.Sum(i => i.PriceAtPurchase * i.Quantity), 2, MidpointRounding.AwayFromZero);
                order.DiscountAmount = Math.Round(await CalculateDiscountAmountAsync(order.OrderItems), 2, MidpointRounding.AwayFromZero);
                order.TaxAmount = Math.Round(await CalculateTaxAmountAsync(order.SubTotalAmount - order.DiscountAmount), 2, MidpointRounding.AwayFromZero);
                order.ShippingCharges = Math.Round(CalculateShippingCharges(order.SubTotalAmount - order.DiscountAmount), 2, MidpointRounding.AwayFromZero);
                order.TotalAmount = Math.Round(order.SubTotalAmount - order.DiscountAmount + order.TaxAmount + order.ShippingCharges, 2, MidpointRounding.AwayFromZero);

                // Save order to repository
                var addedOrder = await _orderRepository.AddAsync(order);
                if (addedOrder == null)
                    throw new InvalidOperationException("Failed to create order.");

                // Initiate payment via Payment Service
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

                // For COD, immediately reserve stock and send notification
                if (request.PaymentMethod == PaymentMethodEnum.COD)
                {
                    #region Event Publishing to RabbitMQ

                    // Construct the integration event (OrderPlacedEvent)
                    var orderPlacedEvent = new OrderPlacedEvent
                    {
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        UserId = order.UserId,
                        CustomerName = user.FullName,
                        CustomerEmail = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        TotalAmount = order.TotalAmount,
                        Items = order.OrderItems.Select(i => new OrderItemLine
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.PriceAtPurchase
                        }).ToList()
                    };

                    // Publish the event to RabbitMQ using the shared publisher.
                    // This message will be routed to:
                    //    - ProductService (to decrease stock)
                    //    - NotificationService (to insert notification record).
                    // correlationId is set for traceability across logs and microservices.
                    await _publisher.PublishOrderPlacedAsync(orderPlacedEvent, Guid.NewGuid().ToString());

                    #endregion

                    // Map and return order DTO with confirmed status and no payment URL
                    var orderDto = _mapper.Map<OrderResponseDTO>(order);
                    orderDto.OrderStatus = OrderStatusEnum.Confirmed;
                    orderDto.PaymentMethod = PaymentMethodEnum.COD;
                    orderDto.PaymentUrl = null;
                    return orderDto;
                }
                else
                {
                    // Map and return order DTO with pending status and payment URL
                    var orderDto = _mapper.Map<OrderResponseDTO>(order);
                    orderDto.OrderStatus = OrderStatusEnum.Pending;
                    orderDto.PaymentMethod = request.PaymentMethod;
                    orderDto.PaymentUrl = paymentResponse.PaymentUrl;
                    return orderDto;
                }
            }
            catch (Exception ex)
            {
                //Log the Exception
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // Confirms an order after successful payment: updates status, reduces stock, sends notifications.
        public async Task<bool> ConfirmOrderAsync(Guid orderId, string accessToken)
        {
            // Retrieve order
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // Only allow confirmation if order is pending
            if (order.OrderStatusId != (int)OrderStatusEnum.Pending)
                throw new InvalidOperationException("Order is not in a pending state.");

            // Retrieve payment info from Payment Service
            var paymentInfo = await _paymentServiceClient.GetPaymentInfoAsync(
                new PaymentInfoRequestDTO { OrderId = orderId }, accessToken);

            if (paymentInfo == null)
                throw new InvalidOperationException("Payment information not found for this order.");

            if (paymentInfo.PaymentStatus != PaymentStatusEnum.Completed)
                throw new InvalidOperationException("Payment is not successful.");

            var user = await _userServiceClient.GetUserByIdAsync(order.UserId, accessToken);
            if (user == null)
                throw new InvalidOperationException("User does not exist.");

            try
            {
                // Change order status to Confirmed
                bool statusChanged = await _orderRepository.ChangeOrderStatusAsync(
                    orderId, OrderStatusEnum.Confirmed, "PaymentService", "Payment successful, order confirmed.");

                if (!statusChanged)
                    throw new InvalidOperationException("Failed to update order status.");

                // Now that the order is confirmed, publish an integration event

                // Create the event payload that downstream services need
                var orderPlacedEvent = new OrderPlacedEvent
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    CustomerName = user.FullName,
                    CustomerEmail = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    TotalAmount = order.TotalAmount,
                    Items = order.OrderItems.Select(i => new OrderItemLine
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.PriceAtPurchase
                    }).ToList()
                };

                // Publish the event to RabbitMQ
                // - _publisher abstracts RabbitMQ communication
                // - The message is sent to exchange "ecommerce.topic" with routing key "order.placed"
                // - ProductService will consume this event to reduce stock
                // - NotificationService will consume this event to insert a notification
                // - correlationId (Guid.NewGuid().ToString()) helps trace this message across logs and services
                await _publisher.PublishOrderPlacedAsync(orderPlacedEvent, Guid.NewGuid().ToString());

                return true;
            }
            catch (Exception ex)
            {
                //Log the Exception
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        // Change order status with full validation, transaction support, and history tracking.
        // Returns detailed response DTO with success or error info.
        public async Task<ChangeOrderStatusResponseDTO> ChangeOrderStatusAsync(ChangeOrderStatusRequestDTO request)
        {
            var response = new ChangeOrderStatusResponseDTO
            {
                OrderId = request.OrderId,
                NewStatus = request.NewStatus,
                ChangedBy = request.ChangedBy,
                Remarks = request.Remarks,
                ChangedAt = DateTime.UtcNow,
                Success = false
            };

            try
            {
                // Fetch order to check current status and existence
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    response.ErrorMessage = "Order not found.";
                    return response;
                }

                var oldStatus = (OrderStatusEnum)order.OrderStatusId;
                response.OldStatus = oldStatus;

                // Prevent setting to the same status
                if (oldStatus == request.NewStatus)
                {
                    response.ErrorMessage = "Order is already in the requested status.";
                    return response;
                }

                // Change status in repository
                bool statusChanged = await _orderRepository.ChangeOrderStatusAsync(
                    request.OrderId,
                    request.NewStatus,
                    request.ChangedBy ?? "System",
                    request.Remarks);

                if (!statusChanged)
                {
                    response.ErrorMessage = "Failed to update order status.";
                    return response;
                }

                // TODO: Trigger notifications or other side-effects based on new status

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                // Catch and return error message instead of throwing
                response.ErrorMessage = $"Exception: {ex.Message}";
                return response;
            }
        }

        // Retrieves an order by ID
        public async Task<OrderResponseDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return null;

            return _mapper.Map<OrderResponseDTO>(order);
        }

        // Retrieves paginated orders for a user.
        public async Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
            var totalCount = orders.Count; // Consider separate count method for true total

            var orderDtos = _mapper.Map<List<OrderResponseDTO>>(orders);

            return new PaginatedResultDTO<OrderResponseDTO>
            {
                Items = orderDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResultDTO<OrderResponseDTO>> GetOrdersAsync(OrderFilterRequestDTO filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var (orders, totalCount) = await _orderRepository.GetOrdersWithFiltersAsync(
                status: filter.Status,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                searchOrderNumber: filter.SearchTerm,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            var orderDtos = _mapper.Map<List<OrderResponseDTO>>(orders);

            return new PaginatedResultDTO<OrderResponseDTO>
            {
                Items = orderDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        // Retrieves the full order status history.
        public async Task<List<OrderStatusHistoryResponseDTO>> GetOrderStatusHistoryAsync(Guid orderId)
        {
            var history = await _orderRepository.GetOrderStatusHistoryAsync(orderId);
            return _mapper.Map<List<OrderStatusHistoryResponseDTO>>(history);
        }

        #region Private Helpers

        // Calculate total discount amount from product-level and order-level discounts.
        private async Task<decimal> CalculateDiscountAmountAsync(IEnumerable<OrderItem> orderItems)
        {
            decimal productLevelDiscountTotal = 0m;

            // Sum discounts applied on individual products (multiplied by quantity)
            foreach (var item in orderItems)
            {
                productLevelDiscountTotal += (item.PriceAtPurchase - item.DiscountedPrice) * item.Quantity;
            }

            decimal orderLevelDiscount = 0m;
            DateTime today = DateTime.UtcNow.Date;

            // Retrieve currently active order-level discounts
            var activeOrderDiscounts = await _masterDataRepository.GetActiveDiscountsAsync(today);

            // Select the best discount (dummy logic: highest percentage or fixed amount)
            var bestOrderDiscount = activeOrderDiscounts
                .Where(d => d.IsActive && d.StartDate <= today && d.EndDate >= today)
                .OrderByDescending(d => d.DiscountType == DiscountTypeEnum.Percentage
                    ? d.DiscountPercentage ?? 0
                    : d.DiscountAmount ?? 0)
                .FirstOrDefault();

            if (bestOrderDiscount != null)
            {
                decimal orderSubtotal = orderItems.Sum(i => i.DiscountedPrice * i.Quantity);
                if (bestOrderDiscount.DiscountType == DiscountTypeEnum.Percentage && bestOrderDiscount.DiscountPercentage.HasValue)
                {
                    orderLevelDiscount = orderSubtotal * (bestOrderDiscount.DiscountPercentage.Value / 100m);
                }
                else if (bestOrderDiscount.DiscountType == DiscountTypeEnum.FixedAmount && bestOrderDiscount.DiscountAmount.HasValue)
                {
                    orderLevelDiscount = bestOrderDiscount.DiscountAmount.Value;
                }
            }

            // Return sum of product-level and order-level discounts
            return productLevelDiscountTotal + orderLevelDiscount;
        }

        // Calculate total tax based on active tax rules applied to the taxable amount.
        private async Task<decimal> CalculateTaxAmountAsync(decimal taxableAmount)
        {
            decimal totalTax = 0m;
            DateTime today = DateTime.UtcNow.Date;

            var activeTaxes = await _masterDataRepository.GetActiveTaxesAsync(today);

            // Sum applicable taxes only on products (can be extended to shipping)
            foreach (var tax in activeTaxes)
            {
                if (tax.IsActive && (!tax.ValidTo.HasValue || tax.ValidTo >= today) && tax.ValidFrom <= today)
                {
                    if (tax.AppliesToProduct)
                    {
                        totalTax += taxableAmount * (tax.TaxPercentage / 100m);
                    }
                }
            }

            return totalTax;
        }

        // Calculates shipping charges based on configuration and order total thresholds.
        private decimal CalculateShippingCharges(decimal orderTotal)
        {
            bool isShippingAllowed = _configuration.GetValue<bool>("ShippingConfig:IsShippingChargeAllowed");
            decimal freeShippingThreshold = _configuration.GetValue<decimal>("ShippingConfig:FreeShippingThreshold");
            decimal ShippingCharge = _configuration.GetValue<decimal>("ShippingConfig:ShippingCharge");

            if (!isShippingAllowed)
                return 0m;

            if (orderTotal >= freeShippingThreshold)
                return 0m;

            return ShippingCharge; // Default shipping charge, configurable if desired
        }

        private string GenerateOrderNumberFromGuid(Guid orderId)
        {
            var prefix = "ORD";
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            // Get last 8 chars from GUID string without dashes
            var guidString = orderId.ToString("N"); // N = digits only, no dashes
            var guidSuffix = guidString.Substring(guidString.Length - 8, 8).ToUpper();

            return $"{prefix}{datePart}{guidSuffix}";
        }

        #endregion
    }
}
