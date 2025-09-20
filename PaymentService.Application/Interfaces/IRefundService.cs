using PaymentService.Application.DTOs;
namespace PaymentService.Application.Interfaces
{
    public interface IRefundService
    {
        Task<RefundResponseDTO> InitiateRefundAsync(RefundRequestDTO request);
        Task<RefundResponseDTO?> GetRefundStatusAsync(Guid refundId);
        Task<RefundQueryResponseDTO> SearchRefundsAsync(RefundFilterRequestDTO request);
        Task<int> ProcessPendingRefundsAsync();
        Task<RefundTransactionResponseDTO?> GetRefundTransactionsAsync(Guid refundId);
        Task<ManualRefundProcessResponseDTO> ProcessRefundManuallyAsync(ManualRefundProcessRequestDTO request);
    }
}
