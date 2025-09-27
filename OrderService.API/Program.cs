using Messaging.Common.Extensions;
using Messaging.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Interfaces;
using OrderService.Application.MappingProfiles;
using OrderService.Application.Mappings;
using OrderService.Application.Services;
using OrderService.Contracts.Messaging;
using OrderService.Infrastructure.DependencyInjection;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using System.Text;
using System.Text.Json.Serialization;

namespace OrderService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
                };
            });

            // This registers our RabbitMqOptions section with the Options pattern in .NET.
            // After this, we can inject IOptions<RabbitMqOptions>(or IOptionsMonitor< RabbitMqOptions >) into any service.
            // This allows us to access RabbitMQ configuration (hostname, username, vhost, etc.) using DI.
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

            // Directly fetch the RabbitMqOptions values from configuration (appsettings.json).
            // This is useful when you need to immediately use these settings during service registration.
            var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

            // Register a RabbitMQ connection/channel with the DI container.
            // AddRabbitMq is a custom extension method (from Messaging.Common.Extensions) that:
            // - Creates a persistent RabbitMQ connection
            // - Creates an IModel (channel)
            // - Registers them as singletons in the DI container
            // This ensures all services reuse the same expensive RabbitMQ connection.
            builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);

            // Register the event publisher implementation as a singleton.
            // IOrderEventPublisher is the contract (interface).
            // RabbitMqOrderEventPublisher is the concrete implementation that publishes OrderPlacedEvent to RabbitMQ.
            // Singleton lifetime is correct because publisher reuses the same RabbitMQ channel for all messages.
            builder.Services.AddSingleton<IOrderEventPublisher, RabbitMqOrderEventPublisher>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
