using AutoMapper;
using OrderService.Application.DTOs.Cancellation;
using OrderService.Application.DTOs.Common;
using OrderService.Application.Interfaces;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.ExternalServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.ExternalServices;

namespace OrderService.Application.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly ICancellationRepository _cancellationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRefundRepository _refundRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IPaymentServiceClient _paymentServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly IMapper _mapper;

        public CancellationService(
            ICancellationRepository cancellationRepository,
            IOrderRepository orderRepository,
            IRefundRepository refundRepository,
            IProductServiceClient productServiceClient,
            IPaymentServiceClient paymentServiceClient,
            INotificationServiceClient notificationServiceClient,
            IMapper mapper)
        {
            _cancellationRepository = cancellationRepository ?? throw new ArgumentNullException(nameof(cancellationRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _refundRepository = refundRepository;
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
            _notificationServiceClient = notificationServiceClient ?? throw new ArgumentNullException(nameof(notificationServiceClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Creates a new cancellation request.
        // - Validates order status and cancellation policy.
        // - For full cancellation, includes all eligible order items.
        // - For partial cancellation, only specified items.
        // - Prevents duplicates and disallows full cancellation if partial already done.
        public async Task<CancellationResponseDTO> CreateCancellationRequestAsync(CreateCancellationRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Fetch order with items and policy
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // Allowed statuses for cancellation request
            var allowedStatuses = new[]
            {
                OrderStatusEnum.Pending,
                OrderStatusEnum.Confirmed,
                OrderStatusEnum.Packed,
                OrderStatusEnum.PartialCancelled
            };

            if (!allowedStatuses.Contains((OrderStatusEnum)order.OrderStatusId))
                throw new InvalidOperationException($"Order status {order.OrderStatusId} does not allow cancellation.");

            // Ensure cancellation policy exists
            var policy = order.CancellationPolicy;
            if (policy == null)
                throw new InvalidOperationException("Cancellation policy not configured for this order.");

            // Check if cancellation window expired
            var cancellationWindowEnd = order.OrderDate.AddDays(policy.AllowedCancellationDays);
            if (DateTime.UtcNow > cancellationWindowEnd)
                throw new InvalidOperationException("Cancellation window expired.");

            // Disallow full cancellation if order is partially cancelled
            if ((OrderStatusEnum)order.OrderStatusId == OrderStatusEnum.PartialCancelled && !request.IsPartial)
                throw new InvalidOperationException("Full cancellation is not allowed after partial cancellation.");

            var cancellationItems = new List<CancellationItem>();

            if (request.IsPartial)
            {
                // Validate partial cancellation request items
                if (request.CancellationItems == null || !request.CancellationItems.Any())
                    throw new ArgumentException("Partial cancellation requires cancellation items.");

                // Get all order items already cancelled or pending cancellation to prevent duplicates
                var existingCancelledItemIds = await _cancellationRepository.GetCancelledOrderItemIdsAsync(order.Id);

                foreach (var itemDto in request.CancellationItems)
                {
                    var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
                    if (orderItem == null)
                        throw new InvalidOperationException($"Order item {itemDto.OrderItemId} not found.");

                    // Check order item status for cancellability
                    var cancellableItemStatuses = new[]
                    {
                        OrderStatusEnum.Pending,
                        OrderStatusEnum.Confirmed,
                        OrderStatusEnum.Packed
                    };

                    if (!cancellableItemStatuses.Contains((OrderStatusEnum)orderItem.ItemStatusId))
                        throw new InvalidOperationException($"Order item {orderItem.Id} cannot be cancelled due to status.");

                    // Check for duplicate cancellation requests
                    if (existingCancelledItemIds.Contains(orderItem.Id))
                        throw new InvalidOperationException($"Order item {orderItem.Id} already has a pending or approved cancellation.");

                    // Validate quantity
                    if (itemDto.Quantity <= 0 || itemDto.Quantity > orderItem.Quantity)
                        throw new InvalidOperationException($"Cancelled quantity for item {orderItem.Id} is invalid.");

                    cancellationItems.Add(new CancellationItem
                    {
                        Id = Guid.NewGuid(),
                        OrderItemId = orderItem.Id,
                        Quantity = itemDto.Quantity
                    });
                }
            }
            else
            {
                // Full cancellation
                foreach (var orderItem in order.OrderItems)
                {
                    cancellationItems.Add(new CancellationItem
                    {
                        Id = Guid.NewGuid(),
                        OrderItemId = orderItem.Id,
                        Quantity = orderItem.Quantity
                    });
                }

                if (!cancellationItems.Any())
                    throw new InvalidOperationException("No cancellable items available for full cancellation.");
            }

            // Prepare Cancellation entity
            var cancellation = new Cancellation
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                UserId = request.UserId,
                ReasonId = request.ReasonId,
                IsPartial = request.IsPartial,
                CancellationPolicyId = policy.Id,
                Remarks = request.Remarks,
                RequestedAt = DateTime.UtcNow,
                CancellationStatusId = (int)CancellationStatusEnum.Pending,
                IsDeleted = false,
                CancellationItems = cancellationItems
            };

            // Save to DB
            var createdCancellation = await _cancellationRepository.AddAsync(cancellation);

            // Notify admin asynchronously

            return _mapper.Map<CancellationResponseDTO>(createdCancellation);
        }

        // Get cancellation by cancellation Id
        public async Task<CancellationResponseDTO?> GetCancellationByIdAsync(Guid cancellationId)
        {
            var cancellation = await _cancellationRepository.GetByIdAsync(cancellationId);
            if (cancellation == null) return null;

            return _mapper.Map<CancellationResponseDTO>(cancellation);
        }


        // Get cancellations by order Id
        public async Task<List<CancellationResponseDTO>?> GetCancellationsByOrderIdAsync(Guid orderId)
        {
            var cancellations = await _cancellationRepository.GetCancellationsByOrderIdAsync(orderId);
            if (cancellations == null) return null;

            return _mapper.Map<List<CancellationResponseDTO>>(cancellations);
        }


        //Update an existing Cancellation
        public async Task<CancellationResponseDTO?> UpdateCancellationAsync(UpdateCancellationRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Fetch existing cancellation from repository
            var existingCancellation = await _cancellationRepository.GetByIdAsync(request.CancellationId);
            if (existingCancellation == null)
                throw new KeyNotFoundException("Cancellation not found.");

            // Only allow update if cancellation status is Pending
            if (existingCancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
                throw new InvalidOperationException("Only cancellations with Pending status can be updated.");

            // Map incoming request DTO to domain entity for update
            // Important: Keep CancellationItems in sync
            var cancellationToUpdate = _mapper.Map<Cancellation>(request);
            cancellationToUpdate.Id = request.CancellationId; // Ensure ID is set correctly

            // Call repository update method
            var updatedCancellation = await _cancellationRepository.UpdateAsync(cancellationToUpdate);

            if (updatedCancellation == null)
                throw new InvalidOperationException("Cancellation update failed.");

            // Return updated cancellation mapped to response DTO
            return _mapper.Map<CancellationResponseDTO>(updatedCancellation);
        }

        //Delete an existing Cancellation, SOFT DELETE
        public async Task DeleteCancellationAsync(Guid cancellationId)
        {
            // Fetch cancellation to check existence and status
            var cancellation = await _cancellationRepository.GetByIdAsync(cancellationId);
            if (cancellation == null)
                throw new KeyNotFoundException("Cancellation not found.");

            // Only allow soft delete if status is Pending
            if (cancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
                throw new InvalidOperationException("Only cancellations with Pending status can be deleted.");

            // Call repository method to perform soft delete
            await _cancellationRepository.DeleteAsync(cancellationId);
        }

        //Get Cancellation Items by Cancellation Id
        public async Task<List<CancellationItemResponseDTO>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId)
        {
            // Validate input
            if (cancellationId == Guid.Empty)
                throw new ArgumentException("Invalid cancellation ID", nameof(cancellationId));

            // Fetch cancellation items from repository
            var cancellationItems = await _cancellationRepository.GetCancellationItemsByCancellationIdAsync(cancellationId);

            // Map entities to DTOs using AutoMapper or manual mapping
            var dtoList = _mapper.Map<List<CancellationItemResponseDTO>>(cancellationItems);

            return dtoList;
        }

        // Get paginated cancellations for a user
        public async Task<PaginatedResultDTO<CancellationResponseDTO>> GetCancellationsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            // Use the dynamic method with only userId filter set
            var (cancellations, totalCount) = await _cancellationRepository.GetCancellationsWithFiltersAsync(
                userId: userId,
                pageNumber: pageNumber,
                pageSize: pageSize);

            var dtoList = _mapper.Map<List<CancellationResponseDTO>>(cancellations);

            return new PaginatedResultDTO<CancellationResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtoList
            };
        }

        // Admin: Get paginated and filtered cancellation list
        public async Task<PaginatedResultDTO<CancellationResponseDTO>> GetCancellationsAsync(CancellationFilterRequestDTO filter)
        {
            // Call the dynamic repository method with all possible filters
            var (cancellations, totalCount) = await _cancellationRepository.GetCancellationsWithFiltersAsync(
                userId: filter.UserId,
                status: filter.Status,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            // Map to response DTO list
            var dtoList = _mapper.Map<List<CancellationResponseDTO>>(cancellations);

            // Return paginated result with total count for pagination UI
            return new PaginatedResultDTO<CancellationResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = dtoList
            };
        }

        // Admin: Approve or Reject cancellation request
        public async Task<CancellationApprovalResponseDTO> ApproveOrRejectCancellationAsync(CancellationApprovalRequestDTO request, string accessToken)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Fetch existing cancellation with minimal data required for validation
                var cancellation = await _cancellationRepository.GetByIdAsync(request.CancellationId);
                if (cancellation == null)
                    throw new KeyNotFoundException("Cancellation request not found.");

                if (cancellation.CancellationStatusId != (int)CancellationStatusEnum.Pending)
                    throw new InvalidOperationException("Cancellation request cannot be processed in its current state.");

                if (request.Status == CancellationStatusEnum.Approved)
                {
                    // Calculate and update cancellation & items amounts
                    CalculateAndUpdateCancellationAmounts(cancellation);

                    // Approve cancellation with updated entity and refund info
                    await _cancellationRepository.ApproveAsync(
                        cancellation,
                        cancellation.IsPartial ? OrderStatusEnum.PartialCancelled : OrderStatusEnum.Cancelled,
                        request.ProcessedBy ?? "System",
                        request.Remarks);

                    var stockUpdates = cancellation.CancellationItems.Select(i => new UpdateStockRequestDTO
                    {
                        ProductId = i.OrderItem.ProductId,
                        Quantity = i.Quantity
                    }).ToList();

                    var stockUpdated = await _productServiceClient.IncreaseStockBulkAsync(stockUpdates, accessToken);
                    if (!stockUpdated)
                        throw new InvalidOperationException("Failed to reserve product stock for COD order.");

                    // Prepare Refund entity (no PaymentMethod)
                    var refund = new Refund
                    {
                        Id = Guid.NewGuid(),
                        OrderId = cancellation.OrderId,
                        CancellationId = cancellation.Id,
                        RefundAmount = cancellation.TotalRefundAmount,
                        RefundDate = DateTime.UtcNow,
                        RefundStatusId = (int)RefundStatusEnum.Pending
                    };
                    await _refundRepository.AddAsync(refund);

                    // Call payment API asynchronously
                    var refundRequest = new RefundRequestDTO
                    {
                        OrderId = cancellation.OrderId,
                        UserId = cancellation.UserId,
                        CancellationId = cancellation.Id,
                        RefundAmount = cancellation.TotalRefundAmount,
                        Reason = "Order cancellation refund"
                    };

                    await _paymentServiceClient.InitiateRefundAsync(refundRequest, accessToken);

                    // Notify user asynchronously
                    _ = _notificationServiceClient.SendOrderCancellationNotificationAsync(cancellation.UserId, cancellation.OrderId, accessToken);

                    return new CancellationApprovalResponseDTO
                    {
                        CancellatioId = cancellation.Id,
                        Status = CancellationStatusEnum.Approved,
                        Remarks = request.Remarks,
                        ProcessedOn = DateTime.UtcNow
                    };
                }
                else
                {
                    // Reject cancellation
                    await _cancellationRepository.RejectAsync(cancellation.Id, request.ProcessedBy ?? "System", request.Remarks);

                    return new CancellationApprovalResponseDTO
                    {
                        CancellatioId = cancellation.Id,
                        Status = CancellationStatusEnum.Rejected,
                        Remarks = request.Remarks,
                        ProcessedOn = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void CalculateAndUpdateCancellationAmounts(Cancellation cancellation)
        {
            if (cancellation.Policy == null)
                throw new InvalidOperationException("Cancellation policy must be set.");

            decimal penaltyPercent = cancellation.Policy.PenaltyPercentage;
            decimal totalPurchase = 0m;
            decimal totalRefund = 0m;

            foreach (var item in cancellation.CancellationItems)
            {
                decimal purchaseAmt = Math.Round((item.OrderItem?.DiscountedPrice ?? 0m) * item.Quantity, 2);
                decimal penalty = Math.Round(purchaseAmt * (penaltyPercent / 100m), 2);
                decimal refundAmt = Math.Round(purchaseAmt - penalty, 2);

                item.PurchaseAmount = item.OrderItem?.DiscountedPrice ?? 0m;
                item.CancellationCharge = penalty;
                item.RefundAmount = refundAmt > 0 ? refundAmt : 0m;

                totalPurchase = Math.Round(totalPurchase + purchaseAmt, 2);
                totalRefund = Math.Round(totalRefund + item.RefundAmount, 2);
            }

            cancellation.PurchaseTotalAmount = totalPurchase;
            cancellation.TotalRefundAmount = totalRefund > 0 ? totalRefund : 0m;
            cancellation.CancellationCharge = (cancellation.PurchaseTotalAmount - cancellation.TotalRefundAmount);
        }
    }
}
