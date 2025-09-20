using RabbitMQ.Client;

namespace Messaging.Common.Connection
{
    public class ConnectionManager
    {
        // Private field: holds the RabbitMQ connection factory (used to create connections).
        private readonly ConnectionFactory _factory;

        // Private field: keeps a reference to the active connection.
        // The question mark (?) means it can be null initially.
        private IConnection? _connection;

        // Constructor: initializes the connection factory with RabbitMQ settings.
        public ConnectionManager(string hostName, string userName, string password, string vhost)
        {
            // Create a new ConnectionFactory instance with the given configuration.
            _factory = new ConnectionFactory
            {
                // The hostname or IP of the RabbitMQ broker (e.g., localhost or a server name).
                HostName = hostName,

                // Username to authenticate with RabbitMQ (e.g., ecommerce_user).
                UserName = userName,

                // Password for the above username.
                Password = password,

                // The virtual host (vhost) in RabbitMQ to connect to (e.g., ecommerce_vhost).
                VirtualHost = vhost,

                // Important: enables async consumers instead of the older sync consumer model. 
                // This is the modern and recommended way in .NET.
                DispatchConsumersAsync = true
            };
        }

        // This method returns an open RabbitMQ connection.
        // If no connection exists or the existing one is closed, it creates a new one.
        public IConnection GetConnection()
        {
            // Check if the _connection is null OR closed
            if (_connection == null || !_connection.IsOpen)
            {
                // Create a new connection using the factory.
                // This is an expensive operation, so we only do it when needed.
                _connection = _factory.CreateConnection();
            }

            // Return the active connection (either existing or newly created).
            return _connection;
        }
    }
}

