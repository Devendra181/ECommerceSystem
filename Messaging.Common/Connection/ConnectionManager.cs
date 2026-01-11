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


//---------------------------------------------
//High level
//---------------------------------------------
//•	ConnectionFactory: the configuration object that knows how to open TCP connections to a RabbitMQ broker (host, port, username, password, vhost, heartbeat, SSL, etc.). You call its CreateConnection(...) to open a network connection.

//•	IConnection: represents a live, open TCP connection to the RabbitMQ broker. It is the expensive, long‑lived resource you should reuse.

//What each one does (plain terms)

    //•	ConnectionFactory
        //•	Holds connection settings (HostName, UserName, Password, VirtualHost, DispatchConsumersAsync, requested heartbeat, SSL options, etc.).
        //•	Is used to create IConnection instances via CreateConnection().
        //•	Lightweight and typically created once and reused.

    //•	IConnection
        //•	Is the actual network connection (socket/TLS) to RabbitMQ.
        //•	Manages heartbeat and frames, and multiplexes channels.
        //•	Exposes events like ConnectionShutdown you can subscribe to.
        //•	Has IsOpen to check whether the connection is still valid.
        //•	When closed or broken it must be recreated.

//---------------------------------------------
//Why this matters (best practices)
//---------------------------------------------
//•	Create one long‑lived IConnection per process (or per app instance). Opening connections is expensive.
//•	Reuse the IConnection across components (publishers/consumers).
//•	Create short‑lived IModel (channel) objects from the IConnection for each logical unit of work. Channels are lightweight compared to connections but are NOT thread‑safe — do not share a channel across threads without synchronization.
//•	Example usage: var channel = connection.CreateModel();
//•	Monitor connection shutdowns and recreate the connection (or use client automatic recovery features) so consumers/publishers recover gracefully.
//•	Set DispatchConsumersAsync = true on the factory if you use async consumer handlers (as your code does).

//---------------------------------------------
//Common properties you’ll see on ConnectionFactory
//---------------------------------------------

//•	HostName, Port, UserName, Password, VirtualHost
//•	RequestedHeartbeat
//•	AutomaticRecoveryEnabled / TopologyRecoveryEnabled (client automatic-recovery options)
//•	DispatchConsumersAsync (true → use async consumer handlers)

//Typical lifecycle in code (concept)
//•	Build a ConnectionFactory with settings.
//•	Use ConnectionFactory.CreateConnection() once to get IConnection.
//•	For publishing or consuming: call connection.CreateModel() to get IModel (channel), use it, then dispose the channel.
//•	If connection.IsOpen is false → recreate connection with factory.CreateConnection().

//Why your ConnectionManager is correct
//•	It keeps a single ConnectionFactory configured once.
//•	It caches a single IConnection and recreates it when null or closed — matching recommended practice to avoid expensive repeated connection opens.