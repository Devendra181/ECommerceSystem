using APIGateway.DTOs.Common;
using ECommerce.Common.ServiceDiscovery.Resolution;
using OrderService.Application.DTOs.Order;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace APIGateway.Services
{
    // Concrete implementation of IOrderApiClient.
    // Responsibility:
    // - Resolve OrderService URL (service discovery)
    // - Forward the request to OrderService
    // - Forward Authorization token if present
    // - Read and return the response in ApiResponse<T> format

    // Important:
    // This class should NOT contain business logic.
    // It only handles HTTP communication and basic response normalization.
    public class OrderApiClient : IOrderApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceResolver _resolver;
        private readonly ILogger<OrderApiClient> _logger;

        // JSON options used across all HTTP calls to OrderService.
        // - PropertyNameCaseInsensitive: supports different JSON casing
        // - JsonStringEnumConverter: allows enum values in string form ("COD", "UPI") instead of numeric
        // Keeping a static shared instance avoids re-creating options for every request.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            // PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            Converters = { new JsonStringEnumConverter() }
        };

        // IHttpClientFactory: recommended way in ASP.NET Core to create HttpClient safely.
        // IServiceResolver  : resolves microservice base URL dynamically (Consul/Eureka/etc.)
        // ILogger           : logs failures for debugging and monitoring.
        public OrderApiClient(
            IHttpClientFactory httpClientFactory,
            IServiceResolver resolver,
            ILogger<OrderApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _resolver = resolver;
            _logger = logger;
        }

        // Calls OrderService /api/Order to create an order.
        // This method is typically invoked by a GraphQL mutation.
        public async Task<ApiResponse<OrderResponseDTO>> CreateOrderAsync(
            CreateOrderRequestDTO request,
            string? authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Create HttpClient instance from factory.
                // This avoids socket exhaustion and supports proper handler reuse.
                var client = _httpClientFactory.CreateClient();

                // Resolve the current base URI of OrderService using service discovery.
                // This allows load balancing and dynamic instance discovery.
                var orderServiceUri = await _resolver.ResolveServiceUriAsync("OrderService", cancellationToken);

                // Build the final endpoint URL for placing the order.
                // Example: http://orderservice/api/Order
                var requestUri = new Uri(orderServiceUri, "/api/Order");

                // Forward Authorization header to OrderService
                if (!string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    // If already in the format: "Bearer <token>", parse directly.
                    if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
                    else
                        // If only raw token is passed, wrap it as Bearer token.
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", authorizationHeader);
                }

                // Send request to OrderService
                // PostAsJsonAsync serializes request body into JSON using JsonOptions.
                var response = await client.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);

                // Read response into common API wrapper
                // We expect OrderService to return ApiResponse<OrderResponseDTO> JSON.
                // If response body is empty, we create a friendly failure response.
                var apiResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponse<OrderResponseDTO>>(JsonOptions, cancellationToken)
                    ?? ApiResponse<OrderResponseDTO>.FailResponse("OrderService returned an empty response.");

                // Handle Unauthorized explicitly
                // Even if downstream returned a body, unauthorized should be handled clearly.
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return ApiResponse<OrderResponseDTO>.FailResponse(
                        "Unauthorized.",
                        new List<string> { "Invalid or missing token." });
                }

                // Handle mismatch: HTTP failure but body says Success = true
                // This is a safety check to avoid returning "success" when HTTP status is not success.
                if (!response.IsSuccessStatusCode && apiResponse.Success)
                {
                    return ApiResponse<OrderResponseDTO>.FailResponse("OrderService returned failure status.");
                }

                // Return the response as-is (either success or failure with messages).
                return apiResponse;
            }
            catch (Exception ex)
            {
                // Any unexpected issue (service discovery failure, network error, serialization issue, etc.)
                // should be logged and converted into a safe response for the UI.
                _logger.LogError(ex, "CreateOrderAsync failed in OrderApiClient.");

                // Avoid leaking internal stack trace details to client in production.
                // Here we return a friendly message + minimal detail.
                return ApiResponse<OrderResponseDTO>.FailResponse(
                    "Failed to place order.",
                    new List<string> { ex.Message });
            }
        }
    }
}

