using OrderService.Contracts.DTOs;
using OrderService.Contracts.ExternalServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Infrastructure.ExternalServices
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductServiceClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ProductServiceClient");
        }

        public async Task<ProductDTO?> GetProductByIdAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Invalid product ID", nameof(productId));

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<List<ProductDTO>?> GetProductsByIdsAsync(List<Guid> productIds, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/products/GetByIds")
            {
                Content = JsonContent.Create(productIds)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDTO>>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<List<ProductStockVerificationResponseDTO>?> CheckProductsAvailabilityAsync(List<ProductStockVerificationRequestDTO> requestedItems, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/verify-stock")
            {
                Content = JsonContent.Create(requestedItems)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductStockVerificationResponseDTO>>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<bool> IncreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/increase-stock-bulk")
            {
                Content = JsonContent.Create(stockUpdates)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;
        }

        public async Task<bool> DecreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/decrease-stock-bulk")
            {
                Content = JsonContent.Create(stockUpdates)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;
        }
    }
}
