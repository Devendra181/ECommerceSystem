namespace OrderService.Domain.Enums
{
    public enum CancellationStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Refunded = 4,
        PartiallyRefunded = 5
    }
}
