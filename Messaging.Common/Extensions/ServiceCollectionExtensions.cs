using Microsoft.Extensions.DependencyInjection;
using Messaging.Common.Connection;

namespace Messaging.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Extension method for IServiceCollection that registers RabbitMQ connection-related services
        // into the ASP.NET Core Dependency Injection (DI) container.
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,  // "this" means we extend IServiceCollection with our own method
            string hostName,                   // RabbitMQ host (e.g., localhost, or server name)
            string userName,                   // RabbitMQ username (e.g., ecommerce_user)
            string password,                   // RabbitMQ password
            string vhost)                      // RabbitMQ virtual host (e.g., ecommerce_vhost)
        {
            // Create our custom ConnectionManager, which handles RabbitMQ connection logic
            var connectionManager = new ConnectionManager(hostName, userName, password, vhost);

            // Get an active connection from RabbitMQ (if not open, it will create one)
            var connection = connectionManager.GetConnection();

            // From the connection, create a channel (IModel) — this is used to declare queues,
            // exchanges, and to publish/consume messages.
            var channel = connection.CreateModel();

            // Register ConnectionManager as a singleton service — one instance will be reused for the whole application lifetime.
            services.AddSingleton(connectionManager);

            // Register the IConnection itself as a singleton service — shared across the app (expensive to create, so reuse).
            services.AddSingleton(connection);

            // Register the IModel (channel) as a singleton — consumers/publishers can inject this directly.
            services.AddSingleton(channel);

            // Return IServiceCollection so we can chain this method in Program.cs (fluent API style).
            return services;
        }
    }
}
