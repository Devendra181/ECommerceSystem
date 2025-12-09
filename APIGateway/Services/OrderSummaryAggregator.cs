using APIGateway.DTOs.Common;
using APIGateway.DTOs.OrderSummary;
using ECommerce.Common.ServiceDiscovery.Resolution;
using OrderService.Application.DTOs.Order;
using ProductService.Application.DTOs;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserService.Application.DTOs;

namespace APIGateway.Services
{
    // Aggregator service that composes a unified Order Summary response by
    // fetching and merging data from multiple microservices:
    //   • OrderService  → Order details
    //   • UserService   → Customer profile
    //   • ProductService → Product info for ordered items
    //   • PaymentService → Payment status (stubbed for now)

    // This pattern allows API Gateway to act as a single aggregation point,
    // reducing client complexity and network round trips.
    public class OrderSummaryAggregator : IOrderSummaryAggregator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderSummaryAggregator> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConsulServiceResolver _resolver;

        // Common/Global JSON serialization options for all downstream microservice calls
        //  - Case-insensitive property matching
        //  - Preserve original property naming
        //  - Enum values as strings
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            Converters = { new JsonStringEnumConverter() }
        };

        public OrderSummaryAggregator(
            IHttpClientFactory httpClientFactory,
            ILogger<OrderSummaryAggregator> logger,
            IHttpContextAccessor httpContextAccessor,
            IConsulServiceResolver resolver)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _resolver = resolver;
        }

        // Entry point: Aggregates order, user, product, and payment data into a
        // single response model.
        public async Task<OrderSummaryResponseDTO?> GetOrderSummaryAsync(Guid orderId)
        {
            // STEP 1️: Fetch the root entity — Order
            var order = await FetchOrderAsync(orderId);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} not found or invalid.", orderId);
                return null;
            }

            // Prepare initial response container
            var result = new OrderSummaryResponseDTO
            {
                OrderId = order.OrderId,
                Order = new OrderInfoDTO
                {
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    Status = order.OrderStatus.ToString(),
                    SubTotalAmount = order.SubTotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    ShippingCharges = order.ShippingCharges,
                    TaxAmount = order.TaxAmount,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod.ToString()
                }
            };

            // Extract necessary references for dependent calls
            var userId = order.UserId;
            var items = order.Items ?? new List<OrderItemResponseDTO>();

            // STEP 2️: Fetch related entities concurrently
            // Run all external service calls in parallel to reduce total latency.
            var customerTask = FetchCustomerAsync(userId);
            var productsTask = FetchProductsAsync(items);
            var paymentTask = FetchPaymentAsync(orderId);

            await Task.WhenAll(customerTask, productsTask, paymentTask);

            // STEP 3️: Aggregate all results into the final DTO
            // Each fetch method handles its own exceptions and logs appropriately.
            result.Customer = customerTask.Result;
            result.Products = productsTask.Result;
            result.Payment = paymentTask.Result;

            // STEP 4️: Handle Partial Failures
            if (result.Customer == null)
                result.Warnings.Add("Customer details could not be loaded.");
            if (!result.Products.Any())
                result.Warnings.Add("Product details could not be fully loaded.");
            if (result.Payment == null)
                result.Warnings.Add("Payment details are unavailable.");

            result.IsPartial = result.Warnings.Any();
            // Return a unified object even if some data sources failed.
            return result;
        }

        // ---------------------- Downstream Call Helpers ----------------------

        // Fetch order details from the Order microservice
        private async Task<OrderResponseDTO?> FetchOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create HttpClient
                var client = _httpClientFactory.CreateClient();

                // Discover OrderService endpoint base URL from Consul
                var orderServiceUri = await _resolver.ResolveServiceUriAsync("OrderService", cancellationToken);

                // Compose the full request URI, e.g., https://orderservice:5001/api/Order/{orderId}
                var requestUri = new Uri(orderServiceUri, $"/api/Order/{orderId}");

                // Issue the GET request to the OrderService.
                var response = await client.GetAsync(requestUri, cancellationToken);

                // If the order is not found, no need to treat it as an error.
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                // Throw if the status code is not 2xx.
                response.EnsureSuccessStatusCode();

                // Deserialize the wrapped ApiResponse<OrderResponseDTO> payload.
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrderResponseDTO>>(JsonOptions, cancellationToken);

                // Validate the response before using it.
                if (apiResponse?.Success != true || apiResponse.Data is null)
                {
                    _logger.LogWarning("OrderService returned invalid response for {OrderId}", orderId);
                    return null;
                }

                return apiResponse.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch order from OrderService for OrderId: {OrderId}", orderId);
                return null;
            }
        }

        // Fetch customer profile information from UserService
        private async Task<CustomerInfoDTO?> FetchCustomerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Discover UserService base URL from Consul.
                var userServiceUri = await _resolver.ResolveServiceUriAsync("UserService", cancellationToken);
                var requestUri = new Uri(userServiceUri, $"/api/User/profile/{userId}/");

                var response = await client.GetAsync(requestUri, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProfileDTO>>(JsonOptions, cancellationToken);

                if (apiResponse?.Success != true || apiResponse.Data is null)
                {
                    _logger.LogWarning("UserService returned empty response for {UserId}", userId);
                    return null;
                }

                // Map the response to a simplified DTO for API Gateway consumers
                return new CustomerInfoDTO
                {
                    UserId = apiResponse.Data.UserId,
                    FullName = apiResponse.Data.FullName,
                    Email = apiResponse.Data.Email,
                    Mobile = apiResponse.Data.PhoneNumber,
                    ProfilePhotoUrl = apiResponse.Data.ProfilePhotoUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for UserId: {UserId}", userId);
                return null;
            }
        }

        // Fetch product details for all products in the order using bulk lookup
        private async Task<List<OrderProductInfoDTO>> FetchProductsAsync(
            IEnumerable<OrderItemResponseDTO> items,
            CancellationToken cancellationToken = default)
        {
            var result = new List<OrderProductInfoDTO>();

            // Extract distinct product IDs
            var productIds = items
                .Select(i => i.ProductId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!productIds.Any())
                return result;

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Discover ProductService base URI from Consul
                var productServiceUri = await _resolver.ResolveServiceUriAsync("ProductService", cancellationToken);
                var endpointUri = new Uri(productServiceUri, "/api/products/GetByIds");

                // Forward Authorization header from incoming request if available
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    client.DefaultRequestHeaders.Authorization =
                        System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader);
                }

                // POST request to fetch product details in bulk
                var response = await client.PostAsJsonAsync(endpointUri, productIds, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("No products found for IDs: {ProductIds}", string.Join(", ", productIds));
                    return result;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("ProductService returned {StatusCode}. Payload: {Body}", response.StatusCode, body);
                    return result;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDTO>>>(JsonOptions, cancellationToken);

                if (apiResponse?.Success != true || apiResponse.Data is null)
                {
                    _logger.LogWarning("ProductService GetByIds returned empty or invalid data.");
                    return result;
                }

                // Build a quick lookup dictionary for mapping (fast matching between order lines and product data)
                var lookup = apiResponse.Data.ToDictionary(p => p.Id, p => p);

                foreach (var item in items)
                {
                    if (lookup.TryGetValue(item.ProductId, out var product))
                    {
                        result.Add(new OrderProductInfoDTO
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            SKU = product.SKU,
                            ImageUrl = product.PrimaryImageUrl,
                            Quantity = item.Quantity,
                            UnitPrice = item.DiscountedPrice
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Product {ProductId} not found in ProductService response.", item.ProductId);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch products for order items: {Ids}", string.Join(", ", productIds));
                return result;
            }
        }

        // Fetch payment details from PaymentService
        private async Task<PaymentInfoDTO?> FetchPaymentAsync(Guid orderId)
        {
            // NOTE:
            // Currently, there is NO endpoint in PaymentService to get payment details by OrderId.
            // This method returns hardcoded data for demo purposes.
            _logger.LogInformation("Payment details for OrderId {OrderId} are stubbed for now.", orderId);
            await Task.CompletedTask;

            return new PaymentInfoDTO
            {
                PaymentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Status = "Paid",
                Method = "Online",
                PaidOn = DateTime.UtcNow.AddMinutes(-5),
                TransactionReference = "DEMO-TXN-PLACEHOLDER"
            };
        }
    }
}
