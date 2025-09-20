using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Domain.MasterEntities;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentProviderConfigurationRepository : IPaymentProviderConfigurationRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentProviderConfigurationRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentProviderConfigurationMaster?> GetActiveConfigurationByProviderAsync(int gatewayProviderId, int environmentId)
        {
            return await _context.PaymentProviderConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.GatewayProviderId == gatewayProviderId && pc.EnvironmentId == environmentId && pc.IsActive);
        }
    }
}

