using RabbitMQ.Client;
using System.Data.Common;
using System.Reflection.Metadata;

namespace Messaging.Common.Connection
{
    // The ConnectionManager class is responsible for managing a single, reusable connection to the RabbitMQ broker. 
    // Opening a RabbitMQ connection is an expensive operation, so this class ensures that:
    //    Only one connection is created per application instance.
    //    The same connection is reused for all publishers and consumers.
    //    If the connection drops, it will be recreated automatically.

    /// <summary>
    /// Key Points: 
    ///     Creates and manages Singleton RabbitMQ Connections.
    ///     Reuses Connections for Publishers and Consumers.
    ///     Handles Automatic Reconnection logic.
    ///     Reduces Network and Broker Load.
    /// </summary>
    public class ConnectionManager
    {
        // ---------------------------------------------------------------------
        // Private Fields
        // ---------------------------------------------------------------------

        // Holds a reference to the RabbitMQ connection factory.
        // The ConnectionFactory is responsible for creating connections to RabbitMQ
        // with the provided host, username, password, and vhost..
        private readonly ConnectionFactory _factory;

        // Keeps a reference to the currently active RabbitMQ connection.
        // The "?" means it can be null initially (before first use).
        private IConnection? _connection;

        // ---------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------
        // Accepts the RabbitMQ configuration values and sets up a Connection Factory
        // that can be used later to open a connection on demand.

        // Parameters
        //      hostName: The hostname or IP address of the RabbitMQ broker.
        //      userName: The username used for authentication.
        //      password: The password for the given username.
        //      vhost: The RabbitMQ virtual host to connect to.
        public ConnectionManager(string hostName, string userName, string password, string vhost)
        {
            // Create and configure the RabbitMQ connection factory
            // The object that knows how to open connections to the RabbitMQ broker.
            _factory = new ConnectionFactory
            {
                // The address (hostname or IP) of the RabbitMQ server.
                HostName = hostName,

                // Username for authenticating to RabbitMQ.
                // This user must have permission to access the virtual host below.
                UserName = userName,

                // Password for the provided username.
                Password = password,

                // Virtual Host (vhost) acts like a namespace in RabbitMQ
                // that keeps exchanges, queues, and permissions separate per environment or app.
                VirtualHost = vhost,

                // Enables support for asynchronous consumers instead of traditional synchronous consumers.
                // Without this, consumers would process messages synchronously, blocking threads.
                // This flag is essential for modern, high-performance .NET applications.
                DispatchConsumersAsync = true
            };
        }

        // ---------------------------------------------------------------------
        // GetConnection Method
        // ---------------------------------------------------------------------
        // Returns an active RabbitMQ connection.
        // If no connection exists or if the existing one is closed, a new one is created.

        // This ensures that the application always has a valid connection
        // without the overhead of creating new connections frequently.
        public IConnection GetConnection()
        {
            // Check if there is no existing connection OR if it has been closed due to timeout or broker restart.
            // This ensures that the app always has a valid, open connection to work with.
            if (_connection == null || !_connection.IsOpen)
            {
                // Logically, this section only runs once or when a reconnection is needed.
                // Create a new connection using the pre-configured factory.
                // NOTE: Creating a connection is an expensive I/O operation — so we avoid doing it frequently.
                _connection = _factory.CreateConnection();
            }

            // Return the current active connection (either existing or newly created).
            // All publishers, consumers, and topology setup classes use this shared connection.
            return _connection;
        }
    }
}

