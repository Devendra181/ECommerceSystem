using Microsoft.Extensions.DependencyInjection;
using Messaging.Common.Connection;

namespace Messaging.Common.Extensions
{
    // Provides an extension method for IServiceCollection that simplify the registration
    // of RabbitMQ connection-related services (ConnectionManager, IConnection, and IModel)
    // into the ASP.NET Core Dependency Injection (DI) container.

    // Purpose:
    // - Centralizes RabbitMQ setup logic in one reusable place.
    // - Keeps Program.cs clean and consistent across microservices.
    // - Allows easy injection of RabbitMQ dependencies into any class (publisher, consumer, topology initializer, etc.).
    public static class ServiceCollectionExtensions
    {
        // Adds and configures RabbitMQ connection services in the DI container.
        // Once registered, you can inject:
        //   - ConnectionManager : For managing the connection lifecycle
        //   - IConnection       : The shared RabbitMQ connection
        //   - IModel            : A lightweight channel used for publishing and consuming messages

        // Parameters:
        //      services: The IServiceCollection instance being extended.
        //      hostName: RabbitMQ server host or IP (e.g., "localhost").
        //      userName: Username for RabbitMQ authentication.
        //      password: Password for the given username.
        //      vhost: The RabbitMQ Virtual Host (vhost) to connect to.

        // Returns:
        //      The same IServiceCollection for fluent configuration chaining.

        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,  // "this" makes it an extension method usable as services.AddRabbitMq(...)
            string hostName,                   // Hostname or server address where RabbitMQ is running
            string userName,                   // RabbitMQ login username
            string password,                   // RabbitMQ login password
            string vhost)                      // Virtual host name (used for logical isolation)
        {
            // --------------------------------------------------------------------
            // Step 1: Create and Configure the ConnectionManager
            // --------------------------------------------------------------------
            // The ConnectionManager is our custom helper class that manages RabbitMQ connections.
            // It handles creating and reusing a single long-lived connection to the RabbitMQ broker.
            var connectionManager = new ConnectionManager(hostName, userName, password, vhost);

            // --------------------------------------------------------------------
            // Step 2: Establish a Connection to RabbitMQ
            // --------------------------------------------------------------------
            // Get an active RabbitMQ connection.
            // If a connection doesn't exist or is closed, ConnectionManager will create a new one.
            // Creating a new connection is expensive (opens a connection to the broker),
            // so we keep it alive and reuse it as long as the application is running.
            var connection = connectionManager.GetConnection();

            // --------------------------------------------------------------------
            // Step 3: Create a Channel (IModel)
            // --------------------------------------------------------------------
            // Channels are used for declaring exchanges/queues, publishing messages, and consuming messages.
            // In this setup, we create one shared channel per application instance
            // that can be injected into any class (like Publisher or BaseConsumer).
            var channel = connection.CreateModel();

            // --------------------------------------------------------------------
            // Step 4: Register Components in the Dependency Injection (DI) Container
            // --------------------------------------------------------------------
            // Register each RabbitMQ-related service as a Singleton.
            // Singleton means one instance will be created and reused for the entire application lifetime.

            // Register ConnectionManager as a Singleton
            // Ensures only one instance manages the connection lifecycle for the entire app.
            services.AddSingleton(connectionManager);

            // Register the RabbitMQ IConnection object.
            // This represents the active TCP connection to RabbitMQ.
            // We register it as a Singleton because it’s resource-intensive to create.
            services.AddSingleton(connection);

            // Register the IModel (channel)
            // The channel is used by publishers and consumers to send/receive messages.
            // Also registered as a Singleton for reuse
            services.AddSingleton(channel);

            // --------------------------------------------------------------------
            // Step 5: Return the Service Collection
            // --------------------------------------------------------------------
            // Returning IServiceCollection allows fluent method chaining, for example:
            // services.AddRabbitMq(...).AddSingleton<IMyService, MyService>();
            return services;
        }
    }
}
