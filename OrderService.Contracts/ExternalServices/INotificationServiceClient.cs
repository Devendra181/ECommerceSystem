namespace OrderService.Contracts.ExternalServices
{
    public interface INotificationServiceClient
    {
        Task SendOrderPlacedNotificationAsync(Guid userId, Guid orderId, string accessToken);
        Task SendOrderCancellationNotificationAsync(Guid userId, Guid orderId, string accessToken);
        Task SendOrderRefundNotificationAsync(Guid userId, Guid orderId, string accessToken);
        Task SendOrderReturnNotificationAsync(Guid userId, Guid orderId, string accessToken);
    }
}
