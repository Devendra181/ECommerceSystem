using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Interfaces;
using OrderService.Application.MappingProfiles;
using OrderService.Application.Mappings;
using OrderService.Application.Services;
using OrderService.Infrastructure.DependencyInjection;
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
