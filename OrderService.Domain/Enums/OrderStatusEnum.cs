namespace OrderService.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 1,
        Confirmed = 2, 
        Packed = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        PartialCancelled = 7,
        Returned = 8,
        PartialReturned = 9
    }
}
