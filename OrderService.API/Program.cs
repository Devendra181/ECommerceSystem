using ECommerce.Common.ServiceDiscovery.Extensions;
using Messaging.Common.Extensions;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Messaging.Common.Topology;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Interfaces;
using OrderService.Application.MappingProfiles;
using OrderService.Application.Mappings;
using OrderService.Application.Messaging;
using OrderService.Application.Services;
using OrderService.Contracts.Messaging;
using OrderService.Infrastructure.DependencyInjection;
using OrderService.Infrastructure.Messaging.Extensions;
using OrderService.Infrastructure.Messaging.Producers;
using OrderService.Infrastructure.Persistence;
using OrdersService.API.Middlewares;
using RabbitMQ.Client;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

namespace OrderService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog from appsettings.json
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();


            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Add DbContext
            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register services
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService.Application.Services.OrderService>();
            builder.Services.AddScoped<ICancellationService, CancellationService>();
            builder.Services.AddScoped<IReturnService, ReturnService>();
            builder.Services.AddScoped<IRefundService, RefundService>();
            builder.Services.AddScoped<IShipmentService, ShipmentService>();

            // Add AutoMapper Mapping Profiles
            builder.Services.AddAutoMapper(typeof(CartMappingProfile));
            builder.Services.AddAutoMapper(typeof(OrderMappingProfile));
            builder.Services.AddAutoMapper(typeof(CancellationMappingProfile));
            builder.Services.AddAutoMapper(typeof(ReturnMappingProfile));
            builder.Services.AddAutoMapper(typeof(RefundMappingProfile));
            builder.Services.AddAutoMapper(typeof(ShipmentMappingProfile));

            //Adding JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
                };
            });

            // ------------------------------------------------------------
            // 1️. Load RabbitMQ configuration options
            // ------------------------------------------------------------
            // Reads all RabbitMQ settings from appsettings.json.
            // These settings are bound to the strongly - typed RabbitMqOptions class.
            // - The first line registers the configuration section with DI
            //   so it can be injected using IOptions<RabbitMqOptions>.
            // - The second line retrieves the same options immediately for use below.
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
            var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

            // ------------------------------------------------------------
            // 2️. Register RabbitMQ Connection and Channel
            // ------------------------------------------------------------
            // - AddRabbitMq() is an extension method from Messaging.Common.Extensions.
            // - It internally:
            //     * Creates a single RabbitMQ connection using the provided host credentials.
            //     * Opens a shared channel (IModel) used by both publishers and consumers.
            //     * Registers these as singletons in the DI container.
            // - This avoids creating multiple connections/channels unnecessarily.
            // - The shared channel improves performance and stability across publishers and consumers.
            builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);

            // ------------------------------------------------------------
            // 3️. Register Core Publisher
            // ------------------------------------------------------------
            // - IPublisher is an abstraction that defines a simple method to publish messages
            //   (PublishAsync(exchange, routingKey, message, correlationId)).
            // - Publisher is the concrete implementation that uses RabbitMQ under the hood.
            builder.Services.AddSingleton<IPublisher, Publisher>();

            // ------------------------------------------------------------
            // 4️. Register Domain-Specific Event Publisher
            // ------------------------------------------------------------
            // - IOrderPlacedEventPublisher is a domain-level abstraction for publishing
            //   the "OrderPlacedEvent" specifically from the OrderService.
            // - OrderPlacedEventPublisher implements this interface and internally calls
            //   IPublisher.PublishAsync() using the exchange and routing key defined
            //   in RabbitMqOptions (e.g., "ecommerce.topic" + "order.placed").
            builder.Services.AddSingleton<IOrderPlacedEventPublisher, OrderPlacedEventPublisher>();

            // ------------------------------------------------------------
            // 5️. Register Compensation Handler
            // ------------------------------------------------------------
            // - IOrderCancelledHandler is implemented by OrderCancelledHandler.
            // - This handler defines what to do when the OrchestratorService
            //   sends an "OrderCancelledEvent" (e.g., due to stock or payment failure).
            // - It updates the order status in the database to "Cancelled"
            //   — this is the Saga compensation step for OrderService.
            builder.Services.AddScoped<IOrderCancelledHandler, OrderCancelledHandler>();

            // ------------------------------------------------------------
            // 6️. Register RabbitMQ Consumer for Compensation Messages
            // ------------------------------------------------------------
            // - The AddOrderCancelledConsumer() extension method (from Infrastructure.Messaging)
            //   registers the OrderCancelledConsumer as a background service.
            // - The consumer:
            //     * Listens to the queue defined by QOrderCompensationCancelled (e.g., "order.compensation_cancelled").
            //     * Consumes OrderCancelledEvent messages published by OrchestratorService.
            //     * Invokes IOrderCancelledHandler in the Application layer for compensation logic.
            // - HostedService ensures it runs continuously in the background and
            //   automatically reconnects if RabbitMQ restarts.
            builder.Services.AddOrderCancelledConsumer();

            // Register Consul for this microservice
            builder.Services.AddConsulRegistration(builder.Configuration);

            var app = builder.Build();

            // ------------------------------------------------------------
            // 7. RabbitMQ Topology Bootstrap (One-time setup at startup)
            // ------------------------------------------------------------
            // - RabbitTopology.EnsureAll() declares exchanges, queues, and bindings
            //   if they do not already exist in RabbitMQ.
            //
            // Why Important?
            // - Prevents runtime errors caused by missing queues or bindings.
            // - Ensures consistent RabbitMQ setup across environments (dev/test/prod).
            //
            // What it does:
            // - Declares the topic exchange (e.g., "ecommerce.topic").
            // - Declares all required queues (like "order.placed", "order.compensation_cancelled").
            // - Binds each queue to the correct routing key.
            //
            // This runs inside a scoped service block to safely access IModel and options.
            using (var scope = app.Services.CreateScope())
            {
                var channel = scope.ServiceProvider.GetRequiredService<IModel>();
                var opttions = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                RabbitTopology.EnsureAll(channel, opttions);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add Correlation Middleware before MapControllers
            app.UseCorrelationId();

            app.MapControllers();

            // Health endpoint used by Consul to check if this instance is alive
            app.MapGet("/health", () => Results.Ok("Healthy"));

            app.Run();
        }
    }
}
