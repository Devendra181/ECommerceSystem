using ECommerce.Common.ServiceDiscovery.Resolution;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.ExternalServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace OrderService.Infrastructure.ExternalServices
{
    public class UserServiceClient : IUserServiceClient
    {

        //private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceResolver _consulServiceResolver;

        public UserServiceClient(IHttpClientFactory httpClientFactory, IServiceResolver consulServiceResolver)
        {
            //_httpClient = httpClientFactory.CreateClient("UserServiceClient");
            _httpClientFactory = httpClientFactory;
            _consulServiceResolver = consulServiceResolver;
        }

        public async Task<bool> UserExistsAsync(Guid userId, string accessToken)
        {
            //Old Withoud Service Discovery

            /* using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{userId}/exists");
             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

             var response = await _httpClient.SendAsync(request);
             if (!response.IsSuccessStatusCode)
                 return false;

             var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
             return apiResponse?.Success == true && apiResponse.Data;  */


            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("UserService");

            var requestUri = new Uri(serviceUri, $"/api/user/{userId}/exists");

            var response = await httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return false;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return apiResponse?.Success == true && apiResponse.Data;
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId, string accessToken)
        {
            //Old Withoud Service Discovery

            /*using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/User/profile/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;*/


            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("UserService");

            var requestUri = new Uri(serviceUri, $"/api/User/profile/{userId}");

            var response = await httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null ;
        }

        public async Task<AddressDTO?> GetUserAddressByIdAsync(Guid userId, Guid addressId, string accessToken)
        {
            //Old Withoud Service Discovery

            /*using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{userId}/address/{addressId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AddressDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;*/

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("UserService");

            var requestUri = new Uri(serviceUri, $"/api/user/{userId}/address/{addressId}");

            var response = await httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AddressDTO>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<Guid?> SaveOrUpdateAddressAsync(AddressDTO addressDto, string accessToken)
        {
            //Old Withoud Service Discovery

            /* using var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/address") // Note endpoint URL: /api/user/address
            {
                Content = JsonContent.Create(addressDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                // You can log or handle failure here if needed
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            return apiResponse?.Success == true ? apiResponse.Data : null; */

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await _consulServiceResolver.ResolveServiceUriAsync("UserService");

            var requestUri = new Uri(serviceUri, "/api/user/address");

            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                // You can log or handle failure here if needed
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            return apiResponse?.Success == true ? apiResponse.Data : null;
        }
    }
}
