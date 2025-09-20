namespace ProductService.Application.DTOs
{
    // For reading product info (Id included)
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string? PrimaryImageAltText { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
