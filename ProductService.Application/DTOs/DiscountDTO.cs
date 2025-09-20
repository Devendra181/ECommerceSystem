namespace ProductService.Application.DTOs
{
    // For Read
    public class DiscountDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid ProductId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
