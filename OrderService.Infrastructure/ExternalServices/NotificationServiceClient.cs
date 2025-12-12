using ECommerce.Common.ServiceDiscovery.Resolution;
using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Contracts.ExternalServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Infrastructure.ExternalServices
{
    public class NotificationServiceClient : INotificationServiceClient
    {

        //private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceResolver consulServiceResolver;

        public NotificationServiceClient(IHttpClientFactory httpClientFactory, IServiceResolver consulServiceResolver)
        {
            //_httpClient = httpClientFactory.CreateClient("NotificationServiceClient");
            this._httpClientFactory = httpClientFactory;
            this.consulServiceResolver = consulServiceResolver;
        }

        public async Task SendOrderPlacedNotificationAsync(Guid userId, Guid orderId, string accessToken)
        {
            var notification = new NotificationRequestDTO
            {
                UserId = userId,
                OrderId = orderId,
                NotificationType = NotificationTypeEnum.OrderPlaced,
                Message = "Your order has been placed successfully."
            };

            await SendNotificationAsync(notification, accessToken);
        }

        public async Task SendOrderCancellationNotificationAsync(Guid userId, Guid orderId, string accessToken)
        {
            var notification = new NotificationRequestDTO
            {
                UserId = userId,
                OrderId = orderId,
                NotificationType = NotificationTypeEnum.OrderCancelled,
                Message = "Your order has been cancelled."
            };

            await SendNotificationAsync(notification, accessToken);
        }

        public async Task SendOrderRefundNotificationAsync(Guid userId, Guid orderId, string accessToken)
        {
            var notification = new NotificationRequestDTO
            {
                UserId = userId,
                OrderId = orderId,
                NotificationType = NotificationTypeEnum.RefundCompleted,
                Message = "Your refund has been processed."
            };

            await SendNotificationAsync(notification, accessToken);
        }

        public async Task SendOrderReturnNotificationAsync(Guid userId, Guid orderId, string accessToken)
        {
            var notification = new NotificationRequestDTO
            {
                UserId = userId,
                OrderId = orderId,
                NotificationType = NotificationTypeEnum.ReturnApproved,
                Message = "Your return request has been approved."
            };

            await SendNotificationAsync(notification, accessToken);
        }

        private async Task SendNotificationAsync(NotificationRequestDTO notification, string accessToken, CancellationToken cancellationToken = default)
        {

            //Old Withoud Service Descovery

            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await _httpClient.PostAsJsonAsync("/api/notifications", notification);
            //if (!response.IsSuccessStatusCode)
            //{
            //    // Log failure or handle as needed, but do not throw here
            //}

            //New Using Consul Service Descovery
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var serviceUri = await consulServiceResolver.ResolveServiceUriAsync("NotificationService", cancellationToken);

            var requestUri = new Uri(serviceUri, "/api/notifications");

            var response = await httpClient.PostAsJsonAsync(requestUri, notification, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Log failure or handle as needed, but do not throw here
            }
        }
    }
}
