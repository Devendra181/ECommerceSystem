using Messaging.Common.Extensions;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Messaging.Common.Topology;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductService.API.Middlewares;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappings;
using ProductService.Application.Messaging;
using ProductService.Application.Services;
using ProductService.Contracts.Messaging;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Messaging.Extensions;
using ProductService.Infrastructure.Messaging.Producers;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json.Serialization;

namespace ProductService.API
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add DbContext
            builder.Services.AddDbContext<ProductDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

            // Register services
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IDiscountService, DiscountService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // -----------------------------------------------------------------------------
            //    RABBITMQ CONFIGURATION & SAGA PATTERN INTEGRATION SECTION
            // -----------------------------------------------------------------------------

            // 1️. Load RabbitMQ configuration (from appsettings.json → "RabbitMq" section)
            //     These options define host, credentials, exchange, routing keys, and queue names
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

            // Extract the strongly-typed RabbitMqOptions object so we can reuse its values directly
            var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

            // 2️. Register RabbitMQ Connection and Channel
            //     - Uses Messaging.Common.Extensions.AddRabbitMq() to:
            //         • Create a single long-lived connection to RabbitMQ.
            //         • Create and register a shared IModel (channel) used by both producers and consumers.
            //     - This ensures each microservice uses consistent topology and avoids connection leaks.
            builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);

            // 3️. Register Core Publisher (Infrastructure-Level Abstraction)
            //     - IPublisher is a shared abstraction defined in Messaging.Common.
            //     - It handles message serialization, setting correlation IDs, and publishing events
            //       to the RabbitMQ exchange. All microservices rely on this same class for consistency.
            builder.Services.AddSingleton<IPublisher, Publisher>();

            // 4️. Register Domain-Specific Publisher (ProductService Responsibility)
            //     - StockReserveEventPublisher implements IStockReserveEventPublisher
            //     - It knows *which routing keys* to use for stock-related events, such as:
            //         • stock.reserved               → when stock successfully reserved.
            //         • stock.reservation_failed     → when stock reservation fails.
            //     - These events are published to the OrchestratorService queues.
            builder.Services.AddSingleton<IStockReserveEventPublisher, StockReserveEventPublisher>();

            // 5️. Register Application Service for Business Logic
            //     - StockReserveService contains the actual logic that checks inventory,
            //       validates quantities, and updates the stock table in the database.
            //     - The Saga pattern calls this service indirectly through the consumer below.
            builder.Services.AddScoped<IStockReserveService, StockReserveService>();

            // 6️. Register RabbitMQ Consumer for Incoming Saga Event
            //     - AddStockReserveConsumer() (defined in ProductService.Infrastructure.Messaging.Extensions)
            //       registers a hosted background service (StockReserveConsumer) that listens to the queue
            //       bound to the routing key "stock.reservation.requested".
            //     - When the OrchestratorService publishes a StockReservationRequestedEvent,
            //       this consumer receives it, processes the stock reservation,
            //       and then publishes either StockReservedCompletedEvent or StockReservationFailedEvent.
            builder.Services.AddStockReserveConsumer();

            var app = builder.Build();

            // -----------------------------------------------------------------------------
            //   TOPOLOGY INITIALIZATION (Ensures Exchange, Queues, and Bindings Exist)
            // -----------------------------------------------------------------------------

            // 7️. During startup, we explicitly ensure that all exchanges, queues, and bindings
            //     are declared using RabbitTopology.EnsureAll(). This is idempotent — safe to call multiple times.
            //     - The ProductService uses the same centralized topology structure as other microservices.
            //     - Ensures this service can immediately start consuming and publishing messages.
            using (var scope = app.Services.CreateScope())
            {
                var ch = scope.ServiceProvider.GetRequiredService<IModel>(); // Active RabbitMQ channel
                var opt = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value; // RabbitMQ options
                RabbitTopology.EnsureAll(ch, opt); // Declare exchange, queues, bindings, and DLX/DLQ setup
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

            app.Run();
        }
    }
}
