using Messaging.Common.Extensions;
using Messaging.Common.Options;
using Messaging.Common.Publishing;
using Messaging.Common.Topology;
using Microsoft.Extensions.Options;
using OrchestratorService.API.Middlewares;
using OrchestratorService.Application.Services;
using OrchestratorService.Contracts.Messaging;
using OrchestratorService.Infrastructure.Messaging.Extensions;
using OrchestratorService.Infrastructure.Messaging.Producers;
using RabbitMQ.Client;
using Serilog;

namespace OrchestratorService.API
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
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // In-memory cache
            // Used by OrchestrationService to store OrderPlaced events temporarily.
            // This is a simple local cache; can later be replaced by Redis for durability.
            builder.Services.AddMemoryCache();

            // RabbitMQ configuration (Strongly typed options)
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
            var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

            // Register RabbitMQ connection + channel
            // Singleton: connection and channel should remain open and reused
            // throughout the service lifetime for efficiency.
            builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);

            // Publisher setup

            // IPublisher → Base abstraction that handles:
            //   - Persistent RabbitMQ connection and channel
            //   - JSON serialization of messages
            // Singleton Reason:
            //   - Uses a long-lived RabbitMQ channel (expensive to create per call)
            //   - Thread-safe for concurrent use
            //   - No per-request or per-message state
            builder.Services.AddSingleton<IPublisher, Publisher>();

            // IOrderEventsPublisher → Orchestrator-specific wrapper around IPublisher
            // Adds routing logic and event-specific publishing methods
            // Singleton Reason:
            //   - Stateless wrapper around IPublisher
            //   - Shares same underlying RabbitMQ channel
            //   - No scoped dependencies or state tracking
            builder.Services.AddSingleton<IOrderEventsPublisher, OrderEventsPublisher>();

            // IOrchestrationService → The Saga Orchestration "brain"
            // Coordinates the workflow between Order, Product, and Notification services.
            // Scoped Reason:
            //   - Runs once per message handling cycle (via consumers)
            //   - May depend on scoped services in future (like DbContext or transactional logic)
            builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

            // Register background consumers for RabbitMQ queues
            // (Each consumer is a hosted background service)
            // Each consumer:
            //   - Is registered as a Singleton HostedService
            //   - Runs continuously, listening to one queue
            //   - Uses IServiceScopeFactory internally to create
            //     scoped lifetimes per message
            builder.Services.AddOrchestratorConsumers();

            var app = builder.Build();

            // 7Ensure RabbitMQ topology
            // Creates all required exchanges, queues, and bindings at startup.
            // This step is idempotent (safe to call even if everything exists).
            using (var scope = app.Services.CreateScope())
            {
                var ch = scope.ServiceProvider.GetRequiredService<IModel>();
                var opt = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                RabbitTopology.EnsureAll(ch, opt);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Add Correlation Middleware before MapControllers
            app.UseCorrelationId();

            app.MapControllers();

            app.Run();
        }
    }
}
