namespace APIGateway.DTOs.OrderSummary
{
    public class CustomerInfoDTO
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }
}
