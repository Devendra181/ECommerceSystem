namespace NotificationService.Domain.Enums
{
    public enum NotificationTypeEnum
    {
        OrderPlaced = 1,
        PaymentSuccess = 2,
        PaymentFailed = 3,
        OrderShipped = 4,
        OrderDelivered = 5,
        OrderCancelled = 6,
        General = 99
    }
}
