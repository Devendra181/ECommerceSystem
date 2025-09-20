namespace OrderService.Domain.Enums
{
    public enum ReturnStatusEnum
    {
        Pending = 1,
        Approved = 2,
        ItemReceived = 3,
        Inspected = 4,
        Refunded = 5,
        Rejected = 6,
        Completed = 7
    }
}
