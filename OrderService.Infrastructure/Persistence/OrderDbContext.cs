using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        DateTime createdAt = new DateTime(2025, 7, 24, 0, 0, 0, DateTimeKind.Utc);

        // Master Entities
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<CancellationStatus> CancellationStatuses { get; set; }
        public DbSet<ReturnStatus> ReturnStatuses { get; set; }
        public DbSet<RefundStatus> RefundStatuses { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }
        public DbSet<ReasonMaster> ReasonMasters { get; set; }
        public DbSet<CancellationPolicy> CancellationPolicies { get; set; }
        public DbSet<ReturnPolicy> ReturnPolicies { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Tax> Taxes { get; set; }

        // Transactional Entities
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<CancellationItem> CancellationItems { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
        
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.OldStatus)
                .WithMany()
                .HasForeignKey(osh => osh.OldStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.NewStatus)
                .WithMany()
                .HasForeignKey(osh => osh.NewStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShipmentStatusHistory>()
                .HasOne(osh => osh.OldShipmentStatus)
                .WithMany()
                .HasForeignKey(osh => osh.OldStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShipmentStatusHistory>()
                .HasOne(osh => osh.NewShipmentStatus)
                .WithMany()
                .HasForeignKey(osh => osh.NewStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CancellationItem>()
                .HasOne(ci => ci.OrderItem)
                .WithMany()
                .HasForeignKey(ci => ci.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReturnItem>()
               .HasOne(ri => ri.Return)
               .WithMany(r => r.ReturnItems)
               .HasForeignKey(ri => ri.ReturnId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShipmentItem>()
                .HasOne(si => si.Shipment)
                .WithMany(s => s.ShipmentItems)
                .HasForeignKey(si => si.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderStatus>()
                .Property(o => o.StatusName)
                .HasConversion<string>();

            builder.Entity<CancellationStatus>()
                .Property(c => c.StatusName)
                .HasConversion<string>();

            builder.Entity<ReturnStatus>()
                .Property(r => r.StatusName)
                .HasConversion<string>();

            builder.Entity<RefundStatus>()
                .Property(r => r.StatusName)
                .HasConversion<string>();

            builder.Entity<ShipmentStatus>()
                .Property(r => r.StatusName)
                .HasConversion<string>();

            builder.Entity<ReasonMaster>()
                .Property(r => r.ReasonType)
                .HasConversion<string>();

            // Seed data for master tables
            SeedOrderStatuses(builder);
            SeedCancellationStatuses(builder);
            SeedReturnStatuses(builder);
            SeedRefundStatuses(builder);
            SeedShipmentStatuses(builder);
            SeedReasonMasters(builder);
            SeedCancellationPolicies(builder);
            SeedReturnPolicies(builder);
            SeedDiscounts(builder);
            SeedTaxes(builder);

            // Configure entity relationships, keys, constraints as needed here
        }

        // Order Statuses
        private void SeedOrderStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, StatusName = OrderStatusEnum.Pending, Description = "Order has been placed but not yet confirmed" },
                new OrderStatus { Id = 2, StatusName = OrderStatusEnum.Confirmed, Description = "Order has been confirmed" },
                new OrderStatus { Id = 3, StatusName = OrderStatusEnum.Packed, Description = "Order has been packed" },
                new OrderStatus { Id = 4, StatusName = OrderStatusEnum.Shipped, Description = "Order has been shipped" },
                new OrderStatus { Id = 5, StatusName = OrderStatusEnum.Delivered, Description = "Order has been delivered" },
                new OrderStatus { Id = 6, StatusName = OrderStatusEnum.Cancelled, Description = "Order has been cancelled" },
                new OrderStatus { Id = 7, StatusName = OrderStatusEnum.PartialCancelled, Description = "Order has been partiallly cancelled" },
                new OrderStatus { Id = 8, StatusName = OrderStatusEnum.Returned, Description = "Order has been returned" },
                new OrderStatus { Id = 9, StatusName = OrderStatusEnum.PartialReturned, Description = "Order has been partiallly returned" }
            );
        }

        // Cancellation Statuses
        private void SeedCancellationStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CancellationStatus>().HasData(
                new CancellationStatus { Id = 1, StatusName = CancellationStatusEnum.Pending, Description = "Cancellation requested" },
                new CancellationStatus { Id = 2, StatusName = CancellationStatusEnum.Approved, Description = "Cancellation approved" },
                new CancellationStatus { Id = 3, StatusName = CancellationStatusEnum.Rejected, Description = "Cancellation rejected" },
                new CancellationStatus { Id = 4, StatusName = CancellationStatusEnum.Refunded, Description = "Refund has been processed" },
                new CancellationStatus { Id = 5, StatusName = CancellationStatusEnum.PartiallyRefunded, Description = "Partial refund has been processed" }
            );
        }

        // Return Statuses
        private void SeedReturnStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReturnStatus>().HasData(
                new ReturnStatus { Id = 1, StatusName = ReturnStatusEnum.Pending, Description = "Return requested" },
                new ReturnStatus { Id = 2, StatusName = ReturnStatusEnum.Approved, Description = "Return approved" },
                new ReturnStatus { Id = 3, StatusName = ReturnStatusEnum.ItemReceived, Description = "Returned item received" },
                new ReturnStatus { Id = 4, StatusName = ReturnStatusEnum.Inspected, Description = "Returned item inspected" },
                new ReturnStatus { Id = 5, StatusName = ReturnStatusEnum.Refunded, Description = "Refund processed" },
                new ReturnStatus { Id = 6, StatusName = ReturnStatusEnum.Rejected, Description = "Return rejected" },
                new ReturnStatus { Id = 7, StatusName = ReturnStatusEnum.Completed, Description = "Return completed" }
            );
        }

        // Refund Statuses
        private void SeedRefundStatuses(ModelBuilder modelBuilder)
        {
            // Refund Statuses
            modelBuilder.Entity<RefundStatus>().HasData(
                new RefundStatus { Id = 1, StatusName = RefundStatusEnum.Pending, Description = "Refund requested but not yet processed" },
                new RefundStatus { Id = 2, StatusName = RefundStatusEnum.Processing, Description = "Refund is being processed" },
                new RefundStatus { Id = 3, StatusName = RefundStatusEnum.Completed, Description = "Refund completed successfully" },
                new RefundStatus { Id = 4, StatusName = RefundStatusEnum.Failed, Description = "Refund failed" },
                new RefundStatus { Id = 5, StatusName = RefundStatusEnum.Cancelled, Description = "Refund request cancelled" }
            );
        }

        // Shipment Statuses
        private void SeedShipmentStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShipmentStatus>().HasData(
                new ShipmentStatus { Id = 1, StatusName = ShipmentStatusEnum.Pending, Description = "Shipment is created but not yet dispatched" },
                new ShipmentStatus { Id = 2, StatusName = ShipmentStatusEnum.Shipped, Description = "Shipment has been shipped from warehouse" },
                new ShipmentStatus { Id = 3, StatusName = ShipmentStatusEnum.InTransit, Description = "Shipment is currently in transit" },
                new ShipmentStatus { Id = 4, StatusName = ShipmentStatusEnum.OutForDelivery, Description = "Shipment is out for delivery with courier" },
                new ShipmentStatus { Id = 5, StatusName = ShipmentStatusEnum.Delivered, Description = "Shipment has been delivered to customer" },
                new ShipmentStatus { Id = 6, StatusName = ShipmentStatusEnum.Returned, Description = "Shipment has been returned to sender" },
                new ShipmentStatus { Id = 7, StatusName = ShipmentStatusEnum.Cancelled, Description = "Shipment was cancelled" }
            );
        }

        // Reason Masters 
        private void SeedReasonMasters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReasonMaster>().HasData(
                // Cancellation Reasons
                new ReasonMaster { Id = 1, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Ordered by mistake" },
                new ReasonMaster { Id = 2, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Found better price elsewhere" },
                new ReasonMaster { Id = 3, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Shipping took too long" },
                new ReasonMaster { Id = 4, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Changed my mind" },
                new ReasonMaster { Id = 5, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Item no longer needed" },
                new ReasonMaster { Id = 6, ReasonType = ReasonTypeEnum.Cancellation, ReasonText = "Payment issues" },

                // Return Reasons
                new ReasonMaster { Id = 7, ReasonType = ReasonTypeEnum.Return, ReasonText = "Product defective" },
                new ReasonMaster { Id = 8, ReasonType = ReasonTypeEnum.Return, ReasonText = "Product not as described" },
                new ReasonMaster { Id = 9, ReasonType = ReasonTypeEnum.Return, ReasonText = "Wrong item delivered" },
                new ReasonMaster { Id = 10, ReasonType = ReasonTypeEnum.Return, ReasonText = "Damaged during shipping" },
                new ReasonMaster { Id = 11, ReasonType = ReasonTypeEnum.Return, ReasonText = "Missing parts or accessories" },
                new ReasonMaster { Id = 12, ReasonType = ReasonTypeEnum.Return, ReasonText = "Product expired or near expiry" },
                new ReasonMaster { Id = 13, ReasonType = ReasonTypeEnum.Return, ReasonText = "Received extra item" }
            );
        }

        // Cancellation Policies
        private void SeedCancellationPolicies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CancellationPolicy>().HasData(
                new CancellationPolicy { Id = 1, PolicyName = "Standard Cancellation Policy", Description = "Cancellations allowed before shipment only", AllowedCancellationDays = 3, PenaltyPercentage = 10m, IsActive = true, CreatedAt = createdAt },
                new CancellationPolicy { Id = 2, PolicyName = "No Cancellation After Shipment", Description = "No cancellations allowed once order is shipped", AllowedCancellationDays = 0, PenaltyPercentage = 100m, IsActive = true, CreatedAt = createdAt },
                new CancellationPolicy { Id = 3, PolicyName = "Flexible Cancellation", Description = "Cancellations allowed up to 24 hours before delivery", AllowedCancellationDays = 1, PenaltyPercentage = 5m, IsActive = true, CreatedAt = createdAt },
                new CancellationPolicy { Id = 4, PolicyName = "No Cancellation - Final Sale", Description = "No cancellations or returns allowed on final sale items", AllowedCancellationDays = 0, PenaltyPercentage = 100m, IsActive = true, CreatedAt = createdAt },
                new CancellationPolicy { Id = 5, PolicyName = "Partial Cancellation Allowed", Description = "Partial cancellation allowed before shipment with 15% penalty", AllowedCancellationDays = 3, PenaltyPercentage = 15m, IsActive = true, CreatedAt = createdAt }
            );
        }

        // Return Policies
        private void SeedReturnPolicies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReturnPolicy>().HasData(
                new ReturnPolicy { Id = 1, PolicyName = "Standard Return Policy", Description = "Returns accepted within 7 days", AllowedReturnDays = 7, RestockingFee = 5m, IsActive = true, CreatedAt = createdAt },
                new ReturnPolicy { Id = 2, PolicyName = "No Returns After 3 Days", Description = "Returns only allowed within 3 days", AllowedReturnDays = 3, RestockingFee = 0m, IsActive = true, CreatedAt = createdAt },
                new ReturnPolicy { Id = 3, PolicyName = "Extended Return Policy", Description = "Returns accepted within 30 days for electronics", AllowedReturnDays = 30, RestockingFee = 10m, IsActive = true, CreatedAt = createdAt },
                new ReturnPolicy { Id = 4, PolicyName = "No Returns on Clearance", Description = "No returns allowed on clearance and discount items", AllowedReturnDays = 0, RestockingFee = 0m, IsActive = true, CreatedAt = createdAt },
                new ReturnPolicy { Id = 5, PolicyName = "Return with Exchange Only", Description = "Returns allowed only if exchanged within 15 days", AllowedReturnDays = 15, RestockingFee = 5m, IsActive = true, CreatedAt = createdAt }
            );
        }

        // Discounts
        private void SeedDiscounts(ModelBuilder builder)
        {
            builder.Entity<Discount>().HasData(
                new Discount { Id = 1, DiscountName = "New Year 10% Off", Description = "10% off on all products", DiscountType = DiscountTypeEnum.Percentage, DiscountPercentage = 10m, MinimumAmount = 500m, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 1, 15), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 2, DiscountName = "Flat $50 Off", Description = "Flat $50 off on orders above $1000", DiscountType = DiscountTypeEnum.FixedAmount, DiscountAmount = 50m, MinimumAmount = 1000m, StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 6, 30), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 3, DiscountName = "Summer Sale 15% Off", Description = "Save 15% on all summer collection", DiscountType = DiscountTypeEnum.Percentage, DiscountPercentage = 15m, MinimumAmount = 0m, StartDate = new DateTime(2025, 7, 1), EndDate = new DateTime(2025, 7, 31), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 4, DiscountName = "Buy 2 Get 1 Free", Description = "Buy two items and get one free on select products", DiscountType = DiscountTypeEnum.FixedAmount, DiscountPercentage = 0m, MinimumAmount = 0m, StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2025, 8, 15), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 5, DiscountName = "Festive Season Flat 20% Off", Description = "Flat 20% discount on orders above $1500 during festive season", DiscountType = DiscountTypeEnum.Percentage, DiscountPercentage = 20m, MinimumAmount = 1500m, StartDate = new DateTime(2025, 10, 1), EndDate = new DateTime(2025, 10, 31), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 6, DiscountName = "Clearance Sale 30% Off", Description = "Up to 30% off on clearance items", DiscountType = DiscountTypeEnum.Percentage, DiscountPercentage = 30m, MinimumAmount = 0m, StartDate = new DateTime(2025, 11, 1), EndDate = new DateTime(2025, 11, 30), IsActive = true, CreatedAt = createdAt },
                new Discount { Id = 7, DiscountName = "New Customer $25 Off", Description = "$25 off on first order above $100", DiscountType = DiscountTypeEnum.FixedAmount, DiscountAmount = 25m, MinimumAmount = 100m, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2026, 1, 1), IsActive = true, CreatedAt = createdAt }
            );
        }

        // Taxes
        private void SeedTaxes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tax>().HasData(
                new Tax { Id = 1, TaxName = "CGST", Description = "Central GST", TaxPercentage = 9m, AppliesToProduct = true, AppliesToShipping = false, IsActive = true, ValidFrom = new DateTime(2023, 1, 1), CreatedAt = createdAt },
                new Tax { Id = 2, TaxName = "SGST", Description = "State GST", TaxPercentage = 9m, AppliesToProduct = true, AppliesToShipping = false, IsActive = true, ValidFrom = new DateTime(2023, 1, 1), CreatedAt = createdAt },
                new Tax { Id = 3, TaxName = "Shipping Tax", Description = "Tax on shipping charges", TaxPercentage = 5m, AppliesToProduct = false, AppliesToShipping = true, IsActive = true, ValidFrom = new DateTime(2023, 1, 1), CreatedAt = createdAt }
            );
        }
    }
}
