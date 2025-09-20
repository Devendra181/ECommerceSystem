using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IGatewayResponseRepository, GatewayResponseRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IUserPaymentMethodRepository, UserPaymentMethodRepository>();
            services.AddScoped<IPaymentProviderConfigurationRepository, PaymentProviderConfigurationRepository>();

            return services;
        }
    }
}

