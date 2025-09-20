using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Domain.Repositories;

namespace PaymentService.Infrastructure.Repositories
{
    public class GatewayResponseRepository : IGatewayResponseRepository
    {
        private readonly PaymentDbContext _context;

        public GatewayResponseRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<GatewayResponse> AddGatewayResponseAsync(GatewayResponse gatewayResponse)
        {
            if (gatewayResponse == null)
                throw new ArgumentNullException(nameof(gatewayResponse));

            gatewayResponse.GatewayResponseId = Guid.NewGuid();
            gatewayResponse.ReceivedAt = DateTime.UtcNow;

            _context.GatewayResponses.Add(gatewayResponse);
            await _context.SaveChangesAsync();

            return gatewayResponse;
        }
    }
}

