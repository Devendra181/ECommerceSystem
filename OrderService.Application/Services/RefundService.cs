using AutoMapper;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Refunds;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly IMapper _mapper;

        public RefundService(IRefundRepository refundRepository, IMapper mapper)
        {
            _refundRepository = refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<RefundResponseDTO?> GetRefundByIdAsync(Guid refundId)
        {
            var refund = await _refundRepository.GetByIdAsync(refundId);
            if (refund == null) return null;

            return _mapper.Map<RefundResponseDTO>(refund);
        }

        public async Task<UpdateRefundStatusResponseDTO> UpdateRefundStatusAsync(UpdateRefundStatusRequestDTO request)
        {

            var updatedRefund = await _refundRepository.UpdateStatusAsync(request.RefundId, request.NewStatus, request.ProcessedBy, request.Remarks);

            return new UpdateRefundStatusResponseDTO()
            {
                RefundId = updatedRefund.Id,
                Status = (RefundStatusEnum)updatedRefund.RefundStatusId,
                Message = request.Remarks,
                ProcessedOn = DateTime.UtcNow
            };
        }

        public async Task DeleteRefundAsync(Guid refundId)
        {
            await _refundRepository.DeleteAsync(refundId);
        }

        public async Task<RefundResponseDTO?> GetByCancellationIdAsync(Guid cancellationId)
        {
            var refund = await _refundRepository.GetByCancellationIdAsync(cancellationId);
            if (refund == null) return null;

            return _mapper.Map<RefundResponseDTO>(refund);
        }

        public async Task<RefundResponseDTO?> GetByReturnIdAsync(Guid returnId)
        {
            var refund = await _refundRepository.GetByReturnIdAsync(returnId);
            if (refund == null) return null;

            return _mapper.Map<RefundResponseDTO>(refund);
        }

        public async Task<PaginatedResultDTO<RefundResponseDTO>> GetRefundsByOrderIdAsync(Guid orderId, int pageNumber = 1, int pageSize = 20)
        {
            var (items, totalCount) = await _refundRepository.GetByOrderIdAsync(orderId, pageNumber, pageSize);
            var dtoList = _mapper.Map<List<RefundResponseDTO>>(items);

            return new PaginatedResultDTO<RefundResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtoList
            };
        }

        public async Task<PaginatedResultDTO<RefundResponseDTO>> GetRefundsAsync(RefundFilterRequestDTO filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var (items, totalCount) = await _refundRepository.GetRefundsWithFiltersAsync(
                userId: filter.UserId,
                status: filter.Status,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize);

            var dtoList = _mapper.Map<List<RefundResponseDTO>>(items);

            return new PaginatedResultDTO<RefundResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = dtoList
            };
        }
    }
}
