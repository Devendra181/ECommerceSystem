using OrderService.Contracts.DTOs;
namespace OrderService.Contracts.ExternalServices
{
    public interface IPaymentServiceClient
    {
        Task<CreatePaymentResponseDTO> InitiatePaymentAsync(CreatePaymentRequestDTO request, string accessToken);
        Task<PaymentInfoResponseDTO?> GetPaymentInfoAsync(PaymentInfoRequestDTO request, string accessToken);
        Task<RefundResponseDTO> InitiateRefundAsync(RefundRequestDTO request, string accessToken);
    }
}
