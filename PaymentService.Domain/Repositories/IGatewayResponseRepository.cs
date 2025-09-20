using PaymentService.Domain.Entities;
namespace PaymentService.Domain.Repositories
{
    public interface IGatewayResponseRepository
    {
        Task<GatewayResponse> AddGatewayResponseAsync(GatewayResponse gatewayResponse);
    }
}
