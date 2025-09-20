using PaymentService.Domain.Entities;
namespace PaymentService.Domain.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> InitiatePaymentAsync(Guid orderId, Guid userId, decimal amount, string currency, Guid? userPaymentMethodId, int paymentMethodTypeId, string? paymentUrl = null);
        Task UpdatePaymentStatusAsync(Guid paymentId, int paymentStatusId);
        Task<Payment?> GetPaymentByOrderAsync(Guid orderId, Guid userId);
        Task<Payment?> GetPaymentByPaymentIdAsync(Guid paymentId);
        Task<IEnumerable<Payment>> GetAllPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetAllPendingPaymentsForRetryAsync();
        Task UpdatePaymentUrlAsync(Guid paymentId, string? paymentUrl);
        Task UpdatePaymentMethodTypeAsync(Guid paymentId, int paymentMethodTypeId, Guid? userPaymentMethodId, bool clearPaymentUrlOnCod = true);
    }
}
