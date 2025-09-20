using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Contracts.ExternalServices;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("UserServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:UserServiceUrl"]
                    ?? throw new ArgumentNullException("UserServiceUrl not configured");
                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("ProductServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:ProductServiceUrl"]
                    ?? throw new ArgumentNullException("ProductServiceUrl not configured");
                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("PaymentServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:PaymentServiceUrl"]
                    ?? throw new ArgumentNullException("PaymentServiceUrl not configured");
                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("NotificationServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:NotificationServiceUrl"]
                    ?? throw new ArgumentNullException("NotificationServiceUrl not configured");
                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddScoped<IUserServiceClient, UserServiceClient>();
            services.AddScoped<IProductServiceClient, ProductServiceClient>();
            services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();
            services.AddScoped<INotificationServiceClient, NotificationServiceClient>();

            services.AddScoped<ICancellationRepository, CancellationRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IMasterDataRepository, MasterDataRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IReturnRepository, ReturnRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();

            return services;
        }
    }
}
