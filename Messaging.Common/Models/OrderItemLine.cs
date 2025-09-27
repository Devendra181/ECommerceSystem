namespace Messaging.Common.Models
{
    public sealed class OrderItemLine
    {
        public Guid ProductId { get; set; }   // Which product
        public int Quantity { get; set; }     // How many units
        public decimal UnitPrice { get; set; }// Price per unit at time of order
    }
}
