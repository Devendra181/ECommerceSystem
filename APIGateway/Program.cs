using APIGateway.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Text;

namespace APIGateway
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // JWT Authentication (edge validation when token is present)
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
                        ),

                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();


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

            // ---------------------------------------------------------------
            // Load Ocelot Configuration
            // ---------------------------------------------------------------
            // Ocelot uses a JSON file (ocelot.json) that defines all routes —
            // mapping between client-facing (Upstream) URLs and internal microservice (Downstream) URLs.
            //
            // optional:false  → ensures ocelot.json must exist; app won’t start without it.
            // reloadOnChange:true → allows automatic route updates during development
            //                       without restarting the API Gateway.
            builder.Configuration.AddJsonFile(
                "ocelot.json",
                optional: false,
                reloadOnChange: true
            );

            // ---------------------------------------------------------------
            // Register Ocelot Services
            // ---------------------------------------------------------------
            // This adds all required Ocelot services (middleware, configuration providers,
            // route matching, downstream request handling, etc.) to the DI container.
            //
            // Passing builder.Configuration allows Ocelot to access the ocelot.json content.
            builder.Services.AddOcelot(builder.Configuration);

            var app = builder.Build();

            app.UseHttpsRedirection();

            // ---------------------------------------------------------------------
            // Add custom middleware before Ocelot.
            // ---------------------------------------------------------------------
            //
            // UseCorrelationId()
            //    → Attaches a unique correlation ID (X-Correlation-ID) to every request.
            //      This value is pushed into Serilog’s LogContext so you can trace
            //      the same request across multiple microservices easily.
            //
            // UseRequestResponseLogging()
            //    → Logs the request and response bodies, masking sensitive fields
            //      (passwords, tokens, etc.), and includes timing metrics.
            app.UseCorrelationId();
            app.UseRequestResponseLogging();

            app.UseGatewayBearerValidation();

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

            await app.UseOcelot();

            app.Run();
        }
    }
}
