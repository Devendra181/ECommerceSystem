using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Returns;
using OrderService.Application.Interfaces;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.ExternalServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IReturnRepository _returnRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRefundRepository _refundRepository;
        private readonly IPaymentServiceClient _paymentServiceClient;
        private readonly IProductServiceClient _productServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly IMapper _mapper;

        public ReturnService(
            IReturnRepository returnRepository,
            IOrderRepository orderRepository,
            IRefundRepository refundRepository,
            IProductServiceClient productServiceClient,
            IPaymentServiceClient paymentServiceClient,
            INotificationServiceClient notificationServiceClient,
            IMapper mapper)
        {
            _returnRepository = returnRepository ?? throw new ArgumentNullException(nameof(returnRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _refundRepository = refundRepository;
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
            _notificationServiceClient = notificationServiceClient ?? throw new ArgumentNullException(nameof(notificationServiceClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Create new return request with validations similar to cancellation
        public async Task<ReturnResponseDTO> CreateReturnRequestAsync(CreateReturnRequestDTO request)
        {
            if (request == null) 
                throw new ArgumentNullException(nameof(request));

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var allowedStatuses = new[]
            {
                OrderStatusEnum.Delivered,
                OrderStatusEnum.PartialReturned 
            };

            if (!allowedStatuses.Contains((OrderStatusEnum)order.OrderStatusId))
                throw new InvalidOperationException($"Order status {order.OrderStatusId} does not allow return.");

            var policy = order.ReturnPolicy;
            if (policy == null)
                throw new InvalidOperationException("Return policy not configured for this order.");

            if (!order.DeliveryDate.HasValue)
                throw new InvalidOperationException("Delivery date is not set. Cannot calculate return window.");

            var returnWindowEnd = order.DeliveryDate.Value.AddDays(policy.AllowedReturnDays);
            if (DateTime.UtcNow > returnWindowEnd)
                throw new InvalidOperationException("Return window expired.");

            if ((OrderStatusEnum)order.OrderStatusId == OrderStatusEnum.PartialReturned && !request.IsPartial)
                throw new InvalidOperationException("Full return is not allowed after partial return.");

            var returnItems = new List<ReturnItem>();

            if (request.IsPartial)
            {
                if (request.ReturnItems == null || !request.ReturnItems.Any())
                    throw new ArgumentException("Partial return requires return items.");

                var existingReturnedItemIds = await _returnRepository.GetReturnedOrderItemIdsAsync(order.Id);

                foreach (var itemDto in request.ReturnItems)
                {
                    var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
                    if (orderItem == null)
                        throw new InvalidOperationException($"Order item {itemDto.OrderItemId} not found.");

                    // Check order item status for returnability
                    var returnableStatuses = new[]
                    {
                        OrderStatusEnum.Delivered,
                        OrderStatusEnum.PartialReturned
                    };

                    if (!returnableStatuses.Contains((OrderStatusEnum)orderItem.ItemStatusId))
                        throw new InvalidOperationException($"Order item {orderItem.Id} cannot be returned due to status.");

                    if (existingReturnedItemIds.Contains(orderItem.Id))
                        throw new InvalidOperationException($"Order item {orderItem.Id} already has a pending or approved return.");

                    if (itemDto.Quantity <= 0 || itemDto.Quantity > orderItem.Quantity)
                        throw new InvalidOperationException($"Return quantity for item {orderItem.Id} is invalid.");

                    returnItems.Add(new ReturnItem
                    {
                        Id = Guid.NewGuid(),
                        OrderItemId = orderItem.Id,
                        Quantity = itemDto.Quantity
                    });
                }
            }
            else
            {
                foreach (var orderItem in order.OrderItems)
                {
                    returnItems.Add(new ReturnItem
                    {
                        Id = Guid.NewGuid(),
                        OrderItemId = orderItem.Id,
                        Quantity = orderItem.Quantity
                    });
                }

                if (!returnItems.Any())
                    throw new InvalidOperationException("No returnable items available for full return.");
            }

            var returnEntity = new Return
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                UserId = request.UserId,
                ReasonId = request.ReasonId,
                IsPartial = request.IsPartial,
                ReturnPolicyId = policy.Id,
                Remarks = request.Remarks,
                RequestedAt = DateTime.UtcNow,
                ReturnStatusId = (int)ReturnStatusEnum.Pending,
                IsDeleted = false,
                ReturnItems = returnItems
            };

            var createdReturn = await _returnRepository.AddAsync(returnEntity);

            // Notify admin if needed
            // await _notificationServiceClient.NotifyAdminReturnRequestedAsync(createdReturn.Id);

            return _mapper.Map<ReturnResponseDTO>(createdReturn);
        }

        // Get return by Id
        public async Task<ReturnResponseDTO?> GetReturnByIdAsync(Guid returnId)
        {
            var returnEntity = await _returnRepository.GetByIdAsync(returnId);
            if (returnEntity == null) return null;

            return _mapper.Map<ReturnResponseDTO>(returnEntity);
        }


        // Get Returns by order Id
        public async Task<List<ReturnResponseDTO>?> GetReturnsByOrderIdAsync(Guid orderId)
        {
            var returns = await _returnRepository.GetReturnsByOrderIdAsync(orderId);
            if (returns == null) return null;

            return _mapper.Map<List<ReturnResponseDTO>>(returns);
        }


        // Update existing return
        public async Task<ReturnResponseDTO?> UpdateReturnAsync(UpdateReturnRequestDTO request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var existingReturn = await _returnRepository.GetByIdAsync(request.ReturnId);
            if (existingReturn == null)
                throw new KeyNotFoundException("Return not found.");

            if (existingReturn.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Only returns with Pending status can be updated.");

            var returnToUpdate = _mapper.Map<Return>(request);
            returnToUpdate.Id = request.ReturnId; // Ensure ID is set correctly

            var updatedReturn = await _returnRepository.UpdateAsync(returnToUpdate);

            if (updatedReturn == null)
                throw new InvalidOperationException("Return update failed.");

            return _mapper.Map<ReturnResponseDTO>(updatedReturn);
        }

        // Soft delete return
        public async Task DeleteReturnAsync(Guid returnId)
        {
            var returnEntity = await _returnRepository.GetByIdAsync(returnId);
            if (returnEntity == null)
                throw new KeyNotFoundException("Return not found.");

            if (returnEntity.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Only returns with Pending status can be deleted.");

            await _returnRepository.DeleteAsync(returnId);
        }

        // Get return items by return id
        public async Task<List<ReturnItemResponseDTO>> GetReturnItemsByReturnIdAsync(Guid returnId)
        {
            var returnItems = await _returnRepository.GetReturnItemsByReturnIdAsync(returnId);
            return _mapper.Map<List<ReturnItemResponseDTO>>(returnItems);
        }

        // Paginated returns by user
        public async Task<PaginatedResultDTO<ReturnResponseDTO>> GetReturnByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            var (returns, totalCount) = await _returnRepository.GetReturnsWithFiltersAsync(
                userId: userId,
                pageNumber: pageNumber,
                pageSize: pageSize);

            var dtoList = _mapper.Map<List<ReturnResponseDTO>>(returns);

            return new PaginatedResultDTO<ReturnResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtoList
            };
        }

        // Paginated and filtered returns (admin)
        public async Task<PaginatedResultDTO<ReturnResponseDTO>> GetReturnAsync(ReturnFilterRequestDTO filter)
        {
            var (returns, totalCount) = await _returnRepository.GetReturnsWithFiltersAsync(
                userId: filter.UserId,
                status: filter.Status,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            var dtoList = _mapper.Map<List<ReturnResponseDTO>>(returns);

            return new PaginatedResultDTO<ReturnResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = dtoList
            };
        }

        // Approve or reject a return request with refund
        public async Task<ReturnApprovalResponseDTO> ApproveOrRejectReturnAsync(ReturnApprovalRequestDTO request, string accessToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var returnEntity = await _returnRepository.GetByIdAsync(request.ReturnRequestId);
            if (returnEntity == null)
                throw new KeyNotFoundException("Return request not found.");

            if (returnEntity.ReturnStatusId != (int)ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Return request cannot be processed in its current state.");

            if (request.Status == ReturnStatusEnum.Approved)
            {
                // Calculate and update amounts
                CalculateAndUpdateReturnAmounts(returnEntity);

                // Approve with updated return entity and refund amount
                await _returnRepository.ApproveAsync(
                    returnEntity,
                    request.ProcessedBy ?? "System",
                    returnEntity.TotalRefundableAmount,
                    request.Remarks);

                //Update the Stock Quantity
                var stockUpdates = returnEntity.ReturnItems.Select(i => new UpdateStockRequestDTO
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
                    OrderId = returnEntity.OrderId,
                    ReturnId = returnEntity.Id,
                    RefundAmount = returnEntity.TotalRefundableAmount,
                    RefundDate = DateTime.UtcNow,
                    RefundStatusId = (int)RefundStatusEnum.Pending,
                };

                await _refundRepository.AddAsync(refund);

                var refundRequest = new RefundRequestDTO
                {
                    OrderId = returnEntity.OrderId,
                    UserId = returnEntity.UserId,
                    ReturnId = returnEntity.Id,
                    RefundAmount = returnEntity.TotalRefundableAmount,
                    Reason = "Order return refund"
                };

                await _paymentServiceClient.InitiateRefundAsync(refundRequest, accessToken);
                _ = _notificationServiceClient.SendOrderReturnNotificationAsync(returnEntity.UserId, returnEntity.OrderId, accessToken);

                return new ReturnApprovalResponseDTO
                {
                    ReturnRequestId = returnEntity.Id,
                    Status = ReturnStatusEnum.Approved,
                    Remarks = request.Remarks,
                    ProcessedOn = DateTime.UtcNow
                };
            }
            else
            {
                await _returnRepository.RejectAsync(returnEntity.Id, request.ProcessedBy ?? "System", request.Remarks);

                return new ReturnApprovalResponseDTO
                {
                    ReturnRequestId = returnEntity.Id,
                    Status = ReturnStatusEnum.Rejected,
                    Remarks = request.Remarks,
                    ProcessedOn = DateTime.UtcNow
                };
            }
        }

        private void CalculateAndUpdateReturnAmounts(Return returnEntity)
        {
            if (returnEntity.Policy == null)
                throw new InvalidOperationException("Return policy must be set.");

            decimal restockingFeePercent = returnEntity.Policy.RestockingFee;
            decimal totalPurchase = 0m;
            decimal totalRefund = 0m;

            foreach (var item in returnEntity.ReturnItems)
            {
                decimal purchaseAmt = Math.Round((item.OrderItem?.DiscountedPrice ?? 0m) * item.Quantity, 2);
                decimal restockingFee = Math.Round(purchaseAmt * (restockingFeePercent / 100m), 2);
                decimal refundAmt = Math.Round(purchaseAmt - restockingFee, 2);

                item.PurchaseAmount = purchaseAmt;
                item.RestockingFee = restockingFee;
                item.RefundAmount = refundAmt > 0 ? refundAmt : 0m;

                totalPurchase = Math.Round(totalPurchase + purchaseAmt, 2);
                totalRefund = Math.Round(totalRefund + item.RefundAmount, 2);
            }

            returnEntity.PurchaseTotalAmount = totalPurchase;
            returnEntity.TotalRefundableAmount = totalRefund > 0 ? totalRefund : 0m;
            returnEntity.RestockingFee = returnEntity.PurchaseTotalAmount - returnEntity.TotalRefundableAmount;
        }
    }
}
