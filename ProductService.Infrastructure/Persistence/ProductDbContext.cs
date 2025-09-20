using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure self-referencing Category relationship
            builder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Static GUIDs
            var catElectronicsId = new Guid("b1c2e1d5-4f3b-4a96-82d3-1a234567890a");
            var catMobilePhonesId = new Guid("d7f8e3c4-6a7b-4a12-9c9f-2b3456789abc");

            var product1Id = new Guid("e2f3d4c5-7a8b-4b23-8d9e-3c456789abcd");
            var product2Id = new Guid("f3a4b5c6-8b9c-4c34-9e0f-4d56789abcde");
            var product3Id = new Guid("a4b5c6d7-9c0d-4d45-af10-5e6789abcdef");
            var product4Id = new Guid("b5c6d7e8-ad1e-4e56-bf21-6f789abcdef0");

            var image1Id = new Guid("c6d7e8f9-be2f-4f67-c032-789abcdef012");
            var image2Id = new Guid("d7e8f901-cf30-5068-d143-89abcdef0123");
            var image3Id = new Guid("e8f901a2-d041-6179-e254-9abcdef01234");
            var image4Id = new Guid("f901a2b3-e152-728a-f365-abcedf012345");

            var discount1Id = new Guid("a1b2c3d4-1234-5678-9abc-def012345678");
            var discount2Id = new Guid("b2c3d4e5-2345-6789-abcd-ef0123456789");

            // Fixed DateTimes (UTC)
            var createdAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc);

            var discountStart1 = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var discountEnd1 = new DateTime(2025, 7, 31, 23, 59, 59, DateTimeKind.Utc);

            var discountStart2 = new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc);
            var discountEnd2 = new DateTime(2025, 8, 15, 23, 59, 59, DateTimeKind.Utc);

            // Categories
            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = catElectronicsId,
                    Name = "Electronics",
                    Description = "Electronic devices and gadgets",
                    IsActive = true,
                    CreatedOn = createdAt
                },
                new Category
                {
                    Id = catMobilePhonesId,
                    Name = "Mobile Phones",
                    Description = "Smartphones and mobile devices",
                    ParentCategoryId = catElectronicsId,
                    IsActive = true,
                    CreatedOn = createdAt
                }
            );

            // Products
            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = product1Id,
                    Name = "Wireless Headphones Pro",
                    SKU = $"ELE-WIR-2025-{GetGuidSuffix(product2Id)}", // ELE (Electronics) - WIR (Wireless) - 2025 - Hexa value of Product Id
                    Description = "Noise-cancelling wireless headphones",
                    Price = 199.99m,
                    StockQuantity = 150,
                    IsActive = true,
                    CategoryId = catElectronicsId,
                    CreatedOn = createdAt
                },
                new Product
                {
                    Id = product2Id,
                    Name = "Smart TV 55 inch",
                    SKU = $"ELE-SMA-2025-{GetGuidSuffix(product2Id)}", // ELE + SMA (Smart) + 2025
                    Description = "55 inch 4K UHD Smart TV",
                    Price = 799.99m,
                    StockQuantity = 80,
                    IsActive = true,
                    CategoryId = catElectronicsId,
                    CreatedOn = createdAt
                },
                new Product
                {
                    Id = product3Id,
                    Name = "Super Smartphone X1",
                    SKU = $"MOB-SUP-2025-{GetGuidSuffix(product2Id)}", // MOB (Mobile Phones) + SUP (Super) + 2025
                    Description = "Latest Super Smartphone X1 with OLED display",
                    Price = 699.99m,
                    StockQuantity = 100,
                    IsActive = true,
                    CategoryId = catMobilePhonesId,
                    CreatedOn = createdAt
                },
                new Product
                {
                    Id = product4Id,
                    Name = "Budget Smartphone Z5",
                    SKU = $"MOB-BUD-2025-{GetGuidSuffix(product2Id)}", // MOB - BUD (Budget) - 2025
                    Description = "Affordable smartphone with great features",
                    Price = 149.99m,
                    StockQuantity = 200,
                    IsActive = true,
                    CategoryId = catMobilePhonesId,
                    CreatedOn = createdAt
                }
            );

            // Product Images
            builder.Entity<ProductImage>().HasData(
                new ProductImage
                {
                    Id = image1Id,
                    ProductId = product1Id,
                    ImageUrl = "https://example.com/images/headphones_pro.jpg",
                    IsPrimary = true,
                    CreatedOn = createdAt
                },
                new ProductImage
                {
                    Id = image2Id,
                    ProductId = product2Id,
                    ImageUrl = "https://example.com/images/smart_tv_55.jpg",
                    IsPrimary = true,
                    CreatedOn = createdAt
                },
                new ProductImage
                {
                    Id = image3Id,
                    ProductId = product3Id,
                    ImageUrl = "https://example.com/images/smartphone_x1_front.jpg",
                    IsPrimary = true,
                    CreatedOn = createdAt
                },
                new ProductImage
                {
                    Id = image4Id,
                    ProductId = product4Id,
                    ImageUrl = "https://example.com/images/budget_smartphone_z5.jpg",
                    IsPrimary = true,
                    CreatedOn = createdAt
                }
            );

            // Discounts
            builder.Entity<Discount>().HasData(
                new Discount
                {
                    Id = discount1Id,
                    Name = "Summer Sale 15% OFF",
                    ProductId = product1Id,
                    DiscountPercentage = 15m,
                    DiscountAmount = null,
                    StartDate = discountStart1,
                    EndDate = discountEnd1,
                    IsActive = true,
                    Description = "15% off on Wireless Headphones Pro",
                    CreatedOn = createdAt
                },
                new Discount
                {
                    Id = discount2Id,
                    Name = "Special Offer $50 OFF",
                    ProductId = product3Id,
                    DiscountPercentage = 0m,
                    DiscountAmount = 50m,
                    StartDate = discountStart2,
                    EndDate = discountEnd2,
                    IsActive = true,
                    Description = "Flat $50 off on Super Smartphone X1",
                    CreatedOn = createdAt
                }
            );
        }

        // Helper method to get last 4 characters of GUID as string suffix
        private string GetGuidSuffix(Guid id)
        {
            return id.ToString("N").Substring(28, 4).ToUpper(); // last 4 hex chars
        }
    }
}
