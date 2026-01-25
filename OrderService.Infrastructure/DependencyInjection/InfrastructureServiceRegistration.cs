using ECommerce.GrpcContracts.Products;
using ECommerce.GrpcContracts.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Contracts.ExternalServices;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.GrpcClients;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure.DependencyInjection
{
    // ============================================================================
    // Infrastructure Service Registration
    // ----------------------------------------------------------------------------
    // This class is responsible for wiring up ALL infrastructure dependencies:
    //   - gRPC clients
    //   - REST HttpClients
    //   - Repository implementations
    // ============================================================================
    public static class InfrastructureServiceRegistration
    {
        // ------------------------------------------------------------------------
        // Extension method to register infrastructure services
        // ------------------------------------------------------------------------
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Replaced With gRPC Clients Below
            //services.AddHttpClient("UserServiceClient", client =>
            //{
            //    var baseUrl = configuration["ExternalServices:UserServiceUrl"]
            //        ?? throw new ArgumentNullException("UserServiceUrl not configured");
            //    client.BaseAddress = new Uri(baseUrl);
            //});

            //services.AddHttpClient("ProductServiceClient", client =>
            //{
            //    var baseUrl = configuration["ExternalServices:ProductServiceUrl"]
            //        ?? throw new ArgumentNullException("ProductServiceUrl not configured");
            //    client.BaseAddress = new Uri(baseUrl);
            //});




            // ============================================================
            // USER SERVICE & PRODUCT SERVICE → gRPC CLIENTS
            // ============================================================

            // These services are:
            //   - High-frequency
            //   - Internal
            //   - Synchronous
            //
            // gRPC is chosen for:
            //   - Better performance
            //   - Lower latency
            //   - Strong contracts
            // ============================================================

            // -----------------------------
            // UserService gRPC client
            // -----------------------------
            services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
            {
                // Read UserService base URL from configuration
                // Example:
                //   "ExternalServices:UserServiceUrl": "https://localhost:5001"
                var baseUrl = configuration["ExternalServices:UserServiceUrl"]
                    ?? throw new ArgumentNullException("UserServiceUrl not configured");

                // Configure gRPC client endpoint
                o.Address = new Uri(baseUrl);
            });

            // -----------------------------
            // ProductService gRPC client
            // -----------------------------
            services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(o =>
            {
                var baseUrl = configuration["ExternalServices:ProductServiceUrl"]
                    ?? throw new ArgumentNullException("ProductServiceUrl not configured");

                o.Address = new Uri(baseUrl);
            });

            // ============================================================
            // PAYMENT & NOTIFICATION → REST HTTP CLIENTS
            // ============================================================
            // These services are:
            //   - Possibly external
            //   - Lower frequency
            //   - Asynchronous / eventual consistency
            //
            // REST is intentionally retained here.
            // ============================================================

            // -----------------------------
            // PaymentService REST client
            // -----------------------------
            services.AddHttpClient("PaymentServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:PaymentServiceUrl"]
                    ?? throw new ArgumentNullException("PaymentServiceUrl not configured");

                client.BaseAddress = new Uri(baseUrl);
            });

            // -----------------------------
            // NotificationService REST client
            // -----------------------------
            services.AddHttpClient("NotificationServiceClient", client =>
            {
                var baseUrl = configuration["ExternalServices:NotificationServiceUrl"]
                    ?? throw new ArgumentNullException("NotificationServiceUrl not configured");

                client.BaseAddress = new Uri(baseUrl);
            });

            //services.AddScoped<IUserServiceClient, UserServiceClient>();
            //services.AddScoped<IProductServiceClient, ProductServiceClient>();

            // -----------------------------
            // Swap REST with gRPC seamlessly
            // -----------------------------
            services.AddScoped<IUserServiceClient, UserServiceGrpcClient>();
            // OrderService now calls UserService via gRPC

            services.AddScoped<IProductServiceClient, ProductServiceGrpcClient>();
            // OrderService now calls ProductService via gRPC

            // -----------------------------
            // REST-based external services
            // -----------------------------
            services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();
            services.AddScoped<INotificationServiceClient, NotificationServiceClient>();

            // ============================================================
            // DOMAIN REPOSITORIES (UNCHANGED)
            // ============================================================
            // These are internal data access abstractions.
            // ============================================================

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
