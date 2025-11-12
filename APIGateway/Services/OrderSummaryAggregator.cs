using APIGateway.DTOs.Common;
using APIGateway.DTOs.OrderSummary;
using OrderService.Application.DTOs.Order;
using ProductService.Application.DTOs;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserService.Application.DTOs;

namespace APIGateway.Services
{
    // Aggregates order details from multiple microservices (Order, User, Product, Payment)
    // to produce a unified Order Summary response for the client.
    public class OrderSummaryAggregator : IOrderSummaryAggregator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderSummaryAggregator> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Global JSON deserialization options for all downstream calls.
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
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Entry point that aggregates order, customer, product, and payment information
        // from their respective microservices.
        public async Task<OrderSummaryResponseDTO?> GetOrderSummaryAsync(Guid orderId)
        {
            // 1. Fetch Order first — this is the root entity that ties all others.
            var order = await FetchOrderAsync(orderId);
            if (order == null)
                return null;

            // Prepare the aggregate response container.
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

            var userId = order.UserId;
            var items = order.Items ?? new List<OrderItemResponseDTO>();

            // 2️. Call other dependent microservices concurrently
            // to minimize response latency.
            var customerTask = FetchCustomerAsync(userId);
            var productsTask = FetchProductsAsync(items);
            var paymentTask = FetchPaymentAsync(orderId);

            // Run all API calls in parallel (non-blocking)
            await Task.WhenAll(customerTask, productsTask, paymentTask);

            // 3️. Aggregate responses and track partial failures.

            // Customer
            if (customerTask.Result != null)
            {
                result.Customer = customerTask.Result;
            }
            else
            {
                result.IsPartial = true;
                result.Warnings.Add("Customer details could not be loaded.");
            }

            // Products
            if (productsTask.Result.Any())
            {
                result.Products = productsTask.Result;
            }
            else
            {
                result.IsPartial = true;
                result.Warnings.Add("Product details could not be fully loaded.");
            }

            // Payment
            if (paymentTask.Result != null)
            {
                result.Payment = paymentTask.Result;
            }
            else
            {
                result.IsPartial = true;
                result.Warnings.Add("Payment details not available.");
            }

            // Return a unified object even if some data sources failed.
            return result;
        }

        // ---------------------- Downstream Call Helpers ----------------------

        // Fetches order details from the Order microservice.
        private async Task<OrderResponseDTO?> FetchOrderAsync(Guid orderId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("OrderService");
                var httpResponse = await client.GetAsync($"/api/Order/{orderId}");

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;

                httpResponse.EnsureSuccessStatusCode();

                // Deserialize wrapped ApiResponse<OrderResponseDTO>
                var apiResponse =
                    await httpResponse.Content.ReadFromJsonAsync<ApiResponse<OrderResponseDTO>>(JsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data is null)
                {
                    _logger.LogWarning("OrderService returned invalid response for {OrderId}", orderId);
                    return null;
                }

                return apiResponse.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Order for orderId: {OrderId}", orderId);
                return null;
            }
        }

        // Fetches user profile information for the customer who placed the order.
        private async Task<CustomerInfoDTO?> FetchCustomerAsync(Guid userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserService");

                // Call User microservice to get user profile
                var httpResponse = await client.GetAsync($"/api/User/profile/{userId}/");

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;

                httpResponse.EnsureSuccessStatusCode();

                var apiResponse =
                    await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ProfileDTO>>(JsonOptions);

                if (apiResponse?.Success != true || apiResponse.Data is null)
                    return null;

                var p = apiResponse.Data;

                // Map user profile into a lightweight DTO for aggregation
                return new CustomerInfoDTO
                {
                    UserId = p.UserId,
                    FullName = p.FullName,
                    Email = p.Email,
                    Mobile = p.PhoneNumber,
                    ProfilePhotoUrl = p.ProfilePhotoUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user profile for {UserId}", userId);
                return null;
            }
        }

        // Fetches detailed product information for all items in the order.
        // Uses a bulk endpoint to reduce round trips.
        private async Task<List<OrderProductInfoDTO>> FetchProductsAsync(IEnumerable<OrderItemResponseDTO> items)
        {
            var result = new List<OrderProductInfoDTO>();

            // Extract distinct product IDs to avoid redundant requests.
            var productIds = items
                .Select(i => i.ProductId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!productIds.Any())
                return result;

            var client = _httpClientFactory.CreateClient("ProductService");

            // Forward the same Authorization header from the incoming request
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                client.DefaultRequestHeaders.Authorization =
                    System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader);
            }

            HttpResponseMessage httpResponse;

            try
            {
                // POST: /api/products/GetByIds  (Bulk fetch)
                httpResponse = await client.PostAsJsonAsync("/api/products/GetByIds", productIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calling ProductService GetProductByIds for products: {ProductIds}",
                    string.Join(", ", productIds));
                return result;
            }

            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("No products found for IDs: {ProductIds}", string.Join(", ", productIds));
                return result;
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError("ProductService GetProductByIds returned {StatusCode}. Payload: {Body}",
                    httpResponse.StatusCode, body);
                return result;
            }

            ApiResponse<List<ProductDTO>>? apiResponse;

            try
            {
                apiResponse = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<List<ProductDTO>>>(JsonOptions);
            }
            catch (Exception ex)
            {
                var body = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError(ex,
                    "Failed to deserialize ProductService GetProductByIds response. Payload: {Body}",
                    body);
                return result;
            }

            if (apiResponse?.Success != true || apiResponse.Data is null || !apiResponse.Data.Any())
            {
                _logger.LogWarning("ProductService GetProductByIds returned empty/invalid data.");
                return result;
            }

            // Build a lookup dictionary for fast matching between order lines and product data
            var productLookup = apiResponse.Data
                .GroupBy(p => p.Id)
                .ToDictionary(g => g.Key, g => g.First());

            // Map each order item with its corresponding product details
            foreach (var item in items)
            {
                if (!productLookup.TryGetValue(item.ProductId, out var p))
                {
                    _logger.LogWarning("Product {ProductId} from order not returned by ProductService.", item.ProductId);
                    continue;
                }

                result.Add(new OrderProductInfoDTO
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    ImageUrl = p.PrimaryImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.DiscountedPrice // Use order price as the true source of value
                });
            }

            return result;
        }

        // Fetches payment details for the given order.
        // Currently stubbed; to be replaced when PaymentService exposes a GET-by-OrderId API.
        private async Task<PaymentInfoDTO?> FetchPaymentAsync(Guid orderId)
        {
            // NOTE:
            // Currently, there is NO endpoint in PaymentService to get payment details by OrderId.
            // This method returns hardcoded data for demo purposes.

            _logger.LogInformation(
                "Payment details for OrderId {OrderId} are currently stubbed. Integration pending.", orderId);

            await Task.CompletedTask;

            // Hardcoded sample payment (used until PaymentService endpoint is ready)
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
