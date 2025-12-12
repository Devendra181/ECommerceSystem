using Azure.Core;
using ECommerce.Common.ServiceDiscovery.Resolution;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.ExternalServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace OrderService.Infrastructure.ExternalServices
{
    public class ProductServiceClient : IProductServiceClient
    {

        //Old Withoud Service Descovery
        //private readonly HttpClient _httpClient;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConsulServiceResolver _consulServiceResolver;

        public ProductServiceClient(IHttpClientFactory httpClientFactory, IConsulServiceResolver consulServiceResolver)
        {
            _httpClientFactory = httpClientFactory;
            _consulServiceResolver = consulServiceResolver;
            //_httpClient = httpClientFactory.CreateClient("ProductServiceClient");
        }

        public async Task<ProductDTO?> GetProductByIdAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Invalid product ID", nameof(productId));

            //Old Withoud Service Descovery
            /*using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;*/


            var httpClient = _httpClientFactory.CreateClient();

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("ProductService");

            var requestUri = new Uri(serviceUri, $"/api/products/{productId}");

            var response = await httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<List<ProductDTO>?> GetProductsByIdsAsync(List<Guid> productIds, string accessToken)
        {
            //Old Withoud Service Descovery
            /* using var request = new HttpRequestMessage(HttpMethod.Post, "api/products/GetByIds")
             {
                 Content = JsonContent.Create(productIds)
             };
             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

             var response = await _httpClient.SendAsync(request);
             if (!response.IsSuccessStatusCode)
                 return null;

             var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDTO>>>();
             return apiResponse?.Success == true ? apiResponse.Data : null;*/


            //New Using Consul Service Descovery
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("ProductService");

            var requestUri = new Uri(serviceUri, "api/products/GetByIds");

            var response = await httpClient.PostAsJsonAsync(requestUri, productIds);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDTO>>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<List<ProductStockVerificationResponseDTO>?> CheckProductsAvailabilityAsync(List<ProductStockVerificationRequestDTO> requestedItems, string accessToken)
        {
           /* using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/verify-stock")
            {
                Content = JsonContent.Create(requestedItems)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductStockVerificationResponseDTO>>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;*/

            //New Using Consul Service Descovery
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("ProductService");
            var requestUri = new Uri(serviceUri, "/api/inventory/verify-stock");

            var response = await httpClient.PostAsJsonAsync(requestUri, requestedItems);
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductStockVerificationResponseDTO>>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<bool> IncreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken)
        {
            /*using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/increase-stock-bulk")
            {
                Content = JsonContent.Create(stockUpdates)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;*/

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("ProductService");
            var requestUri = new Uri(serviceUri, "/api/inventory/increase-stock-bulk");

            var response = await httpClient.PostAsJsonAsync(requestUri, stockUpdates);
            if (!response.IsSuccessStatusCode)
                return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;
        }

        public async Task<bool> DecreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken)
        {
            /*using var request = new HttpRequestMessage(HttpMethod.Post, "/api/inventory/decrease-stock-bulk")
            {
                Content = JsonContent.Create(stockUpdates)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;*/

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("ProductService");
            var requestUri = new Uri(serviceUri, "/api/inventory/decrease-stock-bulk");

            var response = await httpClient.PostAsJsonAsync(requestUri, stockUpdates);
            if (!response.IsSuccessStatusCode)
                return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true;
        }
    }
}
