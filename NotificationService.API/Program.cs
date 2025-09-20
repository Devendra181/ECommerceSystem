using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Handlers;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Application.Utilities;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using System.Text.Json.Serialization;

namespace NotificationService.API
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
