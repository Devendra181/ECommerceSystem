using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
namespace PaymentService.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentInitiateResponseDTO> InitiatePaymentAsync(PaymentInitiateRequestDTO request);
        Task<PaymentStatusResponseDTO?> GetPaymentStatusAsync(Guid paymentId);
        Task<PaymentInitiateResponseDTO?> RetryPaymentAsync(Guid paymentId, PaymentMethodTypeEnum? methodOverride);
        Task<bool> CancelPaymentAsync(Guid paymentId);
        Task<IEnumerable<PaymentStatusResponseDTO>> GetPendingPaymentsAsync();
        Task<bool> HandlePaymentWebhookAsync(PaymentWebhookEventDTO payload);
        Task<int> ProcessPendingPaymentsAsync();
        Task<PaymentTransactionResponseDTO?> GetPaymentTransactionsAsync(Guid paymentId);
    }
}
