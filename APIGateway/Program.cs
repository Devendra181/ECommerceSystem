using APIGateway.Extensions;
using APIGateway.Middlewares;
using APIGateway.Models;
using APIGateway.ServiceDiscovery;
using APIGateway.Services;
using ECommerce.Common.ServiceDiscovery.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Ocelot.DependencyInjection;
using Serilog;
using System.Text;
using Yarp.ReverseProxy.Configuration;

namespace APIGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Thread.Sleep(TimeSpan.FromSeconds(10)); // Wait for dependent services to be ready (e.g., Order, User, Consul, Redis)

            // MVC Controllers + Newtonsoft JSON Configuration
            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // Newtonsoft.Json used instead of System.Text.Json
                    // because it provides finer control over property naming
                    // and serialization behavior.
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        // Preserve property names as defined in the DTOs.
                        // No camelCasing or snake_casing transformation.
                        NamingStrategy = new DefaultNamingStrategy()
                    };

                    // Optional: Uncomment if you want Enum values serialized as strings
                    // options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            // Load reverseproxy.json (YARP)
            builder.Configuration.AddJsonFile(
                "reverseproxy.json",
                optional: false,
                reloadOnChange: true
            );


            // Register Consul for this microservice
            builder.Services.AddConsulRegistration(builder.Configuration);

            // Register Consul-based YARP config filter
            builder.Services.AddSingleton<IProxyConfigFilter, ConsulConfigFilter>();

            // Register YARP
            //  - Routes + clusters come from reverseproxy.json
            //  - ConsulConfigFilter fills in cluster destinations from Consul
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                .AddConfigFilter<ConsulConfigFilter>();

            // Register Yarp Configuration Refresh Service
            builder.Services.AddHostedService<YarpConfigRefreshService>();

            // Reads the RateLimiting section from appsettings.json
            // Binds it to your RateLimitSettings model
            // Registers the RateLimitPolicyService as a singleton
            builder.Services.AddCustomRateLimiting(builder.Configuration);

            // Bind CompressionSettings section to our model using opions pattern
            builder.Services.Configure<CompressionSettings>(
                builder.Configuration.GetSection("CompressionSettings"));

            // Ocelot Configuration (API Gateway Routing Layer)
            // Comment the following
            // ---------------------------------------------------------------
            // Load Ocelot Configuration
            // ---------------------------------------------------------------
            // Ocelot uses a JSON file (ocelot.json) that defines all routes —
            // mapping between client-facing (Upstream) URLs and internal microservice (Downstream) URLs.
            //
            // optional:false  → ensures ocelot.json must exist; app won’t start without it.
            // reloadOnChange:true → allows automatic route updates during development
            //                       without restarting the API Gateway.
            //builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            // ---------------------------------------------------------------
            // Register Ocelot Services
            // ---------------------------------------------------------------
            // This adds all required Ocelot services (middleware, configuration providers,
            // route matching, downstream request handling, etc.) to the DI container.
            //
            // Passing builder.Configuration allows Ocelot to access the ocelot.json content.
            //builder.Services.AddOcelot(builder.Configuration);


            // Structured Logging Setup (Serilog)
            // ---------------------------------------------------------------------
            // Configure Serilog as the application's main logging provider.
            // ---------------------------------------------------------------------
            //
            // The LoggerConfiguration() object defines how logs are captured,
            // formatted, and written to sinks (e.g., console, file).
            //
            // Serilog supports structured logging, so every log entry can include
            // custom properties (like CorrelationId, Environment, RequestPath, etc.)
            // that make log analysis easier in tools like Seq, Kibana, or Grafana.
            //
            // 1️ .ReadFrom.Configuration(builder.Configuration)
            //     → Reads all Serilog settings (sinks, templates, overrides, etc.)
            //       directly from appsettings.json. This allows configuration
            //       without recompiling code.
            //
            // 2️ .Enrich.FromLogContext()
            //     → Captures context-specific properties pushed via LogContext
            //       (for example, CorrelationId from your custom middleware).
            //
            // 3️ .CreateLogger()
            //     → Finalizes the logger and assigns it to Log.Logger,
            //       making it globally available via Serilog’s static Log class.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            // ---------------------------------------------------------------------
            // Replace the default .NET logging system with Serilog.
            // ---------------------------------------------------------------------
            //
            // By default, ASP.NET Core uses Microsoft.Extensions.Logging
            // which outputs unstructured text logs.
            //
            // The call below tells the host to pipe all framework and application
            // logs through Serilog instead. This ensures consistent, structured
            // log output everywhere.
            builder.Host.UseSerilog(); 

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ---------------------------------------------------------------------
            // JWT Authentication (edge validation when token is present)  (Bearer Token Validation)
            // ---------------------------------------------------------------------
            builder.Services
                .AddAuthentication(options =>
                {
                    // Define the default authentication scheme as Bearer
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Token validation configuration
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

                        // We’re not validating audience because microservices share same gateway.
                        ValidateAudience = false,

                        // Enforce token expiry check
                        ValidateLifetime = true,

                        // Ensure token signature integrity using secret key
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
                        ),

                        // No extra grace period for expired tokens
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization(); // Enables [Authorize] attributes.

            // Downstream Microservice Clients (typed HttpClientFactory)
            // Each downstream service (Order, User, Product, Payment)
            // is registered with a named HttpClient. This allows
            // resilience, pooling, and reuse within DI-based consumers.
            var urls = builder.Configuration.GetSection("ServiceUrls");

            builder.Services.AddHttpClient("OrderService", c =>
            {
                c.BaseAddress = new Uri(urls["OrderService"]!);
            });

            builder.Services.AddHttpClient("UserService", c =>
            {
                c.BaseAddress = new Uri(urls["UserService"]!);
            });

            builder.Services.AddHttpClient("ProductService", c =>
            {
                c.BaseAddress = new Uri(urls["ProductService"]!);
            });

            builder.Services.AddHttpClient("PaymentService", c =>
            {
                c.BaseAddress = new Uri(urls["PaymentService"]!);
            });

            // Register IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Custom Aggregation Service Registration
            // The aggregator composes multiple downstream responses (Order, User,
            // Product, Payment) into a single unified payload.
            builder.Services.AddScoped<IOrderSummaryAggregator, OrderSummaryAggregator>();

            //----------------------------Register Redis -------------------------------
            // Register Redis as the distributed caching provider for the API Gateway.
            //    This allows our middleware (and any service) to store and retrieve cache data 
            //    in a centralized Redis instance instead of in-memory cache.
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // The connection string defines how our app connects to the Redis server.
                //    Example format: "localhost:6379" (for local Redis)
                //    or "redis:6379,password=yourpassword,ssl=False,abortConnect=False" (for containerized/remote setup)
                //    The value is read from appsettings.json → "RedisCacheSettings:ConnectionString".
                options.Configuration = builder.Configuration["RedisCacheSettings:ConnectionString"];

                // The instance name is an optional logical prefix used to differentiate keys
                //    when multiple applications share the same Redis server.
                //    Example: If InstanceName = "ApiGateway_", all cache keys will start with that prefix.
                //    This helps prevent key collisions between different microservices or environments.
                options.InstanceName = builder.Configuration["RedisCacheSettings:InstanceName"];
            });

            // Build WebApplication instance
            var app = builder.Build();

            // Swagger (API Explorer for development/debugging)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Global Cross-Cutting Middleware
            // Applied to ALL requests — both custom /gateway endpoints and
            // proxied Ocelot routes.

            // ---------------------------------------------------------------------
            // Add custom middleware before Ocelot.
            // ---------------------------------------------------------------------

            // 1. Correlation ID (MUST RUN FIRST)
            //
            // UseCorrelationId()
            //    → Attaches a unique correlation ID (X-Correlation-ID) to every request.
            //      This value is pushed into Serilog’s LogContext so you can trace
            //      the same request across multiple microservices easily.

            app.UseCorrelationId();

            // 2. Logging (request/response)
            //
            // UseRequestResponseLogging()
            //    → Logs the request and response bodies, masking sensitive fields
            //      (passwords, tokens, etc.), and includes timing metrics.
            app.UseRequestResponseLogging();

            // 3. Authentication (Validate JWT Signature)
            //
            // All requests NOT starting with /gateway are handled by Ocelot/YARP.
            // These requests are routed to the correct microservice defined in ocelot.json.
            app.UseAuthentication(); // Needed so Ocelot can read HttpContext.User

            // NOTE: Do NOT call UseAuthorization().

            // 4. Validate token BEFORE proxying
            app.UseGatewayBearerValidation();

            // 5. Rate Limiting (after authentication → per user)
            app.UseCustomRateLimiting();

            // 6. Redis Cache (must run before compression)
            app.UseRedisResponseCaching();

            // 7. Compression (must be LAST before proxy)
            app.UseMiddleware<ConditionalResponseCompressionMiddleware>();

            // BRANCH 1: Custom Aggregated Endpoints (/gateway/*)
            // Any route starting with /gateway (e.g. /gateway/order-summary)
            // is handled directly by ASP.NET controllers — not Ocelot.
            app.MapWhen(
                ctx => ctx.Request.Path.StartsWithSegments("/gateway", StringComparison.OrdinalIgnoreCase),
                gatewayApp =>
                {
                    // Enable endpoint routing for this sub-pipeline
                    gatewayApp.UseRouting();

                    // Apply authentication & authorization
                    gatewayApp.UseAuthentication();
                    gatewayApp.UseAuthorization();

                    // Apply rate limiting also inside this sub-pipeline if needed
                    gatewayApp.UseCustomRateLimiting();

                    // Register controller actions under this branch
                    gatewayApp.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            // BRANCH 2: YARP Reverse Proxy
            // YARP must be last
            app.MapReverseProxy();

            // Ocelot middleware handles routing, transformation, and load-balancing
            // ---------------------------------------------------------------
            // Register Ocelot Middleware (Core Gateway Logic)
            // ---------------------------------------------------------------
            // Ocelot middleware is the heart of the API Gateway.
            // What Ocelot Middleware Does:
            //   - Inspects incoming HTTP requests.
            //   - Matches it to a configured route defined in ocelot.json.
            //   - Forwards the request to the correct downstream microservice.
            //   - Collects the downstream response and returns it to the client.
            //
            // IMPORTANT:
            // This MUST be the LAST middleware in the pipeline,
            // Once Ocelot handles a request, no other middleware executes afterward.
            // Comment the following
            // await app.UseOcelot();

            // Health endpoint used by Consul to check if this instance is alive
            app.MapGet("/health", () => Results.Ok("Healthy"));

            // Start the Application
            app.Run();
        }
    }
}
