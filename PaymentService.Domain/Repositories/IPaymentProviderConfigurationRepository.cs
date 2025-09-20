using PaymentService.Domain.MasterEntities;

namespace PaymentService.Domain.Repositories
{
    public interface IPaymentProviderConfigurationRepository
    {
        Task<PaymentProviderConfigurationMaster?> GetActiveConfigurationByProviderAsync(int gatewayProviderId, int environmentId);
    }
}
