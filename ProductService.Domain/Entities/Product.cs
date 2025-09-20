using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Entities
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;
        [Required, MaxLength(50)]
        public string SKU { get; set; } = null!;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<ProductImage> ProductImages { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        //A product to have multiple discounts, such as seasonal sales or flash offers.
        public List<Discount> Discounts { get; set; } = new();

        // Computed property - not stored in DB
        [NotMapped]
        public decimal DiscountedPrice
        {
            get
            {
                var now = DateTime.UtcNow;

                var activeDiscount = Discounts?
                    .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now)
                    .OrderByDescending(d => d.CreatedOn) 
                    .FirstOrDefault();

                if (activeDiscount == null)
                    return Price;

                if (activeDiscount.DiscountPercentage.HasValue && activeDiscount.DiscountPercentage.Value > 0)
                {
                    var discountedPrice = Price - (Price * activeDiscount.DiscountPercentage.Value / 100);
                    discountedPrice = Math.Round(discountedPrice, 2, MidpointRounding.AwayFromZero);
                    return discountedPrice < 0 ? 0 : discountedPrice;
                }

                if (activeDiscount.DiscountAmount.HasValue && activeDiscount.DiscountAmount.Value > 0)
                {
                    var discountedPrice = Price - activeDiscount.DiscountAmount.Value;
                    discountedPrice = Math.Round(discountedPrice, 2, MidpointRounding.AwayFromZero);
                    return discountedPrice < 0 ? 0 : discountedPrice;
                }

                // If neither discount applies, return original price
                return Price;
            }
        }

        [NotMapped]
        public int TotalReviews => Reviews?.Count() ?? 0;

        [NotMapped]
        public decimal AverageRating
        {
            get
            {
                var totalReviews = Reviews?.ToList();

                if (totalReviews == null || totalReviews.Count == 0)
                    return 0; // Or nullable decimal if preferred

                var average = totalReviews.Average(r => r.Rating);
                return Math.Round((decimal)average, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}
