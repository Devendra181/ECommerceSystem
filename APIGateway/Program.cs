using Ocelot.DependencyInjection;
using Ocelot.Middleware;
namespace APIGateway
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // ---------------------------------------------------------------
            // Register Ocelot Middleware (Core Gateway Logic)
            // ---------------------------------------------------------------
            // Ocelot middleware is the heart of the API Gateway.
            // It intercepts every incoming client request and:
            //   - Matches it to a configured route in ocelot.json
            //   - Forwards it to the corresponding downstream microservice
            //   - Returns the downstream response to the client
            // IMPORTANT: This MUST be the LAST middleware in the pipeline,
            // because once Ocelot handles a request, it won’t pass it to any subsequent middleware.
            await app.UseOcelot();

            app.Run();
        }
    }
}
