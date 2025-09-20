namespace OrderService.Contracts.Enums
{
    public enum NotificationTypeEnum
    {
        OrderPlaced,
        PaymentSuccess,
        PaymentFailure,
        OrderCancelled,
        RefundInitiated,
        RefundCompleted,
        ReturnRequested,
        ReturnApproved,
        ReturnRejected
    }
}
