namespace ProductService.Application.DTOs
{
    // For Read
    public class ProductImageDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public string? AltText { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
