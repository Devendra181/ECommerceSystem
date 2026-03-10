using APIGateway.DTOs.OrderSummary;
using APIGateway.Services;
using HotChocolate.Authorization;
namespace APIGateway.GraphQL.Queries
{
    // GraphQL Query resolver class for the API Gateway.
    // Purpose:
    // - Exposes read operations (Queries) to clients through the /graphql endpoint.
    // - Acts as a BFF layer: the UI asks once, and the gateway collects data from multiple services.
    // Note:
    // Each public method becomes a GraphQL query field (based on Hot Chocolate conventions).
    public class GatewayQuery
    {
        // Real-time scenario:
        // The Order Details / Dashboard screen needs a complete OrderSummary in ONE call:
        // - Order info
        // - Customer info
        // - Product list
        // - Payment info

        // Different screens need different data shapes. With GraphQL, the UI can request only the
        // fields it needs from OrderSummary.
        [Authorize] // Ensures only authenticated users can access this query.
        public async Task<OrderSummaryResponseDTO?> GetOrderSummaryAsync(
            Guid orderId,
            [Service] IOrderSummaryAggregator aggregator, // Injected service that collects data from microservices.
            [Service] ILogger<GatewayQuery> logger)       // Injected logger for observability & debugging.
        {
            // Log the query execution start with the orderId for traceability.
            logger.LogInformation(
                "GraphQL Query GetOrderSummary called. OrderId={OrderId}",
                orderId);

            try
            {
                // Call the gateway aggregation service that fetches and combines data
                // from multiple microservices into a single OrderSummaryResponseDTO.
                //
                // This keeps the resolver thin:
                // - Resolver = orchestration entry point
                // - Aggregator/service layer = actual orchestration logic
                var result = await aggregator.GetOrderSummaryAsync(orderId);

                // If result is null, it usually means the order does not exist
                // (or could not be fetched due to downstream service issues).
                if (result == null)
                {
                    logger.LogWarning(
                        "OrderSummary not found. OrderId={OrderId}",
                        orderId);
                }
                else
                {
                    // Success log helps in monitoring and debugging performance issues.
                    logger.LogInformation(
                        "OrderSummary successfully fetched. OrderId={OrderId}",
                        orderId);
                }

                // Returning the DTO allows GraphQL to shape the final response
                // based on what fields the client requested.
                return result;
            }
            catch (Exception ex)
            {
                // Log the full exception for server-side debugging.
                logger.LogError(
                    ex,
                    "Error occurred while fetching OrderSummary. OrderId={OrderId}",
                    orderId);

                // Re-throw so Hot Chocolate can return a GraphQL error response.
                throw;
            }
        }
    }
}

