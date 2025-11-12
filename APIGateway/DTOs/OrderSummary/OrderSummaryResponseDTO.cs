namespace APIGateway.DTOs.OrderSummary
{
    public class OrderSummaryResponseDTO
    {
        public Guid OrderId { get; set; }
        public OrderInfoDTO Order { get; set; } = null!;
        public CustomerInfoDTO? Customer { get; set; }
        public List<OrderProductInfoDTO> Products { get; set; } = new();
        public PaymentInfoDTO? Payment { get; set; }
        public bool IsPartial { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
