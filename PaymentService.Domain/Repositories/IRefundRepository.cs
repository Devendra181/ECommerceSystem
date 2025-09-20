using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
namespace PaymentService.Domain.Repositories
{
    public interface IRefundRepository
    {
        Task<Refund> InitiateRefundAsync(Guid paymentId, Guid? originalTransactionId, int refundMethodTypeId, decimal amount, string reason, Guid initiatedBy);
        Task UpdateRefundStatusAsync(Guid refundId, int refundStatusId, DateTime? processedAt = null, Guid? refundTransactionId = null);
        Task<IEnumerable<Refund>> GetRefundsByPaymentIdAsync(Guid paymentId);
        Task<Refund?> GetRefundsByRefundIdAsync(Guid refundId);
        Task<IEnumerable<Refund>> GetAllPendingRefundsForRetryAsync();
        Task UpdateAsync(Refund refund);
        Task<(int Total, List<Refund> Items)> GetRefundsByFilterAsync(
            RefundStatusEnum? status,
            RefundMethodTypeEnum? refundMethodType,
            DateTime? fromDateUtc,
            DateTime? toDateUtc,
            Guid? paymentId,
            Guid? userId,
            int pageNumber,
            int pageSize);
    }
}
