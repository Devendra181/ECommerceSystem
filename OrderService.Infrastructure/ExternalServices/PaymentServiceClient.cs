using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Contracts.ExternalServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Infrastructure.ExternalServices
{
    public class PaymentServiceClient : IPaymentServiceClient
    {
        private readonly HttpClient _httpClient;

        public PaymentServiceClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PaymentServiceClient");
        }

        public async Task<CreatePaymentResponseDTO> InitiatePaymentAsync(CreatePaymentRequestDTO request, string accessToken)
        {
            // Simulate async delay
            await Task.Delay(100);

            // Return dummy successful response
            return new CreatePaymentResponseDTO
            {
                PaymentId = Guid.NewGuid(),
                Status = PaymentStatusEnum.Pending,
                PaymentUrl = "https://dummy-payment-gateway.com/pay/123456",
                ErrorMessage = null
            };

            //using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/payments/create")
            //{
            //    Content = JsonContent.Create(request)
            //};
            //httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await _httpClient.SendAsync(httpRequest);
            //response.EnsureSuccessStatusCode();

            //var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CreatePaymentResponseDTO>>();
            //return apiResponse?.Data!;
        }

        public async Task<PaymentInfoResponseDTO?> GetPaymentInfoAsync(PaymentInfoRequestDTO request, string accessToken)
        {
            // Simulate async delay
            await Task.Delay(100);

            // Return dummy payment info
            return new PaymentInfoResponseDTO
            {
                OrderId = Guid.NewGuid(),
                PaymentId = Guid.NewGuid(),
                PaymentStatus = PaymentStatusEnum.Completed,
                PaymentMethod = PaymentMethodEnum.CreditCard,
                PaidAmount = 1000.00m,
                PaymentDate = DateTime.UtcNow.AddMinutes(-10),
                TransactionReference = "TXN123456789",
                FailureReason = null
            };


            //using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/payments/info")
            //{
            //    Content = JsonContent.Create(request)
            //};
            //httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await _httpClient.SendAsync(httpRequest);
            //if (!response.IsSuccessStatusCode) return null;

            //var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentInfoResponseDTO>>();
            //return apiResponse?.Success == true ? apiResponse.Data : null;
        }

        public async Task<RefundResponseDTO> InitiateRefundAsync(RefundRequestDTO request, string accessToken)
        {
            // Simulate async delay
            await Task.Delay(100);

            // Return dummy refund response
            return new RefundResponseDTO
            {
                Success = true,
                ErrorMessage = null
            };

            //using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/payments/refund")
            //{
            //    Content = JsonContent.Create(request)
            //};
            //httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await _httpClient.SendAsync(httpRequest);
            //response.EnsureSuccessStatusCode();

            //var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RefundResponseDTO>>();
            //return apiResponse?.Data!;
        }
    }
}
