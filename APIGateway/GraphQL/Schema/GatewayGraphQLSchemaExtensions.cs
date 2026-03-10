using APIGateway.GraphQL.Input.Types;
using APIGateway.GraphQL.Mutations;
using APIGateway.GraphQL.Output.Types;
using APIGateway.GraphQL.Queries;
using OrderService.Contracts.Enums;
using OrderService.Domain.Enums;
namespace APIGateway.GraphQL.Schema
{
    // Extension method to register and configure GraphQL for the API Gateway.
    // - The Gateway acts as a BFF (Backend for Frontend).
    // - UI screens need different shapes of data.
    // - GraphQL provides one endpoint (/graphql) where the client can request only required fields.
    // This registration builds the GraphQL schema (contract) that clients will use.
    public static class GatewayGraphQLSchemaExtensions
    {
        // Adds GraphQL server and registers:
        // - Root operations (Query + Mutation)
        // - Input types (for client request payloads)
        // - Output types (for response shapes)
        // - Enum types (for strict allowed values)
        public static IServiceCollection AddGatewayGraphQL(this IServiceCollection services)
        {
            services
                // Registers Hot Chocolate GraphQL server in ASP.NET Core DI container.
                .AddGraphQLServer()

                // Enables [Authorize] attribute support for Query/Mutation methods.
                // This integrates GraphQL with ASP.NET Core authentication/authorization.
                .AddAuthorization()

                // Root Operation Types (Entry points for clients)
                // Query = read operations (similar to REST GET)
                // Mutation = write operations (similar to REST POST/PUT/DELETE)
                .AddQueryType<GatewayQuery>()
                .AddMutationType<GatewayMutation>()

                // Input Types (Client request shapes)
                // These define how input DTOs appear in GraphQL schema.
                // They control: input object names, field types, nullability, etc.
                .AddType<PlaceOrderInputType>()
                .AddType<OrderItemInputType>()
                .AddType<AddressInputType>()

                // Output Types (Response shapes)
                // These define how response DTOs appear in GraphQL schema.
                // They control: output type names, field types, nested types, nullability, etc.
                // These types are designed to be "screen-ready", meaning UI can fetch
                // exactly what it needs without multiple REST calls.
                .AddType<OrderSummaryOutputType>()
                .AddType<OrderInfoOutputType>()
                .AddType<CustomerInfoOutputType>()
                .AddType<OrderProductInfoOutputType>()
                .AddType<PaymentInfoOutputType>()
                .AddType<OrderOutputType>()
                .AddType<OrderItemOutputType>()
                .AddType<PlaceOrderPayloadType>()
                .AddType<UserErrorType>()

                // Enum Types (Strict allowed values)
                // Exposes C# enums as GraphQL enums.
                // This improves contract clarity because the client can see
                // the exact set of allowed values in schema (instead of random strings).
                .AddType<EnumType<PaymentMethodEnum>>()
                .AddType<EnumType<OrderStatusEnum>>();

            // Return IServiceCollection so this can be chained in Program.cs
            return services;
        }
    }
}
