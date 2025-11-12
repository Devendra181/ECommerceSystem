using APIGateway.DTOs.OrderSummary;
namespace APIGateway.Services
{
    public interface IOrderSummaryAggregator
    {
        Task<OrderSummaryResponseDTO?> GetOrderSummaryAsync(Guid orderId);
    }
}
