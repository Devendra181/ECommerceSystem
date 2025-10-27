using Messaging.Common.Extensions;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.API.Middlewares;
using NotificationService.Application.Handlers;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Messaging;
using NotificationService.Application.Services;
using NotificationService.Application.Utilities;
using NotificationService.Contracts.Interfaces;
using NotificationService.Contracts.Messaging;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.BackgroundJobs;
using NotificationService.Infrastructure.Messaging.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json.Serialization;

namespace NotificationService.API
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

            // Add DbContext
            builder.Services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
            builder.Services.AddScoped<IPreferenceService, PreferenceService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISMSService, SMSService>();
            builder.Services.AddScoped<ITemplateService, TemplateService>();
            builder.Services.AddScoped<ITemplateRenderer, TemplateRenderer>();

            // Channel Handlers
            builder.Services.AddScoped<INotificationChannelHandler, EmailChannelHandler>();
            builder.Services.AddScoped<INotificationChannelHandler, SmsChannelHandler>();
            builder.Services.AddScoped<INotificationChannelHandler, InAppChannelHandler>();

            //Repositories
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
            builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();

            // Register Application service
            builder.Services.AddScoped<INotificationProcessor, NotificationService.Application.Services.NotificationService>();

            // Register Background Worker
            builder.Services.AddHostedService<NotificationWorker>();

            // ============================================================
            // RABBITMQ CONFIGURATION SECTION (SAGA PATTERN INTEGRATION)
            // ============================================================

            // 1️ Load RabbitMQ configuration from appsettings.json into strongly-typed RabbitMqOptions.
            //    This includes settings like host, username, exchange, and queue names.
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

            // 2️ Retrieve the configuration immediately for topology setup (exchange/queue declarations).
            var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

            // 3️ Register RabbitMQ Connection + Channel into the DI container.
            //    This uses a shared extension from Messaging.Common.
            //    - Creates a long-lived connection to RabbitMQ (ConnectionManager)
            //    - Opens a channel (IModel) for publishing and consuming
            //    - Registers both as singletons, reused throughout the service lifetime
            builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);

            // 4️ Register the application-level message handler.
            //    This class (NotificationServiceHandler) receives events from consumers
            //    and handles the business logic for creating and sending notifications.
            builder.Services.AddScoped<INotificationServiceHandler, NotificationServiceHandler>();

            // 5️ Register the RabbitMQ consumers for Saga events.
            //    - OrderConfirmedConsumer → listens to "order.confirmed" messages.
            //    - OrderCancelledConsumer → listens to "order.cancelled" messages.
            //    Each consumer runs as a hosted background service that reacts
            //    automatically to messages published by the OrchestratorService.
            builder.Services.AddNotificationConsumers();

            var app = builder.Build();

            // ============================================================
            // ENSURE RABBITMQ TOPOLOGY (EXCHANGES, QUEUES, BINDINGS)
            // ============================================================
            // On application startup, ensure that the RabbitMQ topology exists.
            // This step:
            //    - Declares the main topic exchange (ecommerce.topic)
            //    - Declares the dead-letter exchange (DLX)
            //    - Creates queues for all consumers (order.confirmed, order.cancelled)
            //    - Binds them to their corresponding routing keys.
            // This is idempotent → safe to call multiple times without duplicating resources.
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
