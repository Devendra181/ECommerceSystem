using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
namespace NotificationService.Infrastructure.Persistence
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        // Core Entities
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        public DbSet<NotificationAttachment> NotificationAttachments { get; set; }
        public DbSet<NotificationAttemptLog> NotificationAttemptLogs { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        // Template Entities
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationTemplateAudit> NotificationTemplateAudits { get; set; }

        // Master Entities
        public DbSet<NotificationChannelMaster> NotificationChannels { get; set; }
        public DbSet<NotificationStatusMaster> NotificationStatuses { get; set; }
        public DbSet<NotificationTypeMaster> NotificationTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Defining Relationships
            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Recipients)
                .WithOne(r => r.Notification)
                .HasForeignKey(r => r.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Attachments)
                .WithOne(a => a.Notification)
                .HasForeignKey(a => a.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.AttemptLogs)
                .WithOne(l => l.Notification)
                .HasForeignKey(l => l.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationTemplate>()
                .HasMany(t => t.AuditTrail)
                .WithOne(a => a.Template)
                .HasForeignKey(a => a.TemplateId);

            //Property Conversion
            modelBuilder.Entity<NotificationChannelMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<NotificationStatusMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<NotificationTypeMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            // === Master Data Seeding ===

            var createdAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            // Notification Channels
            modelBuilder.Entity<NotificationChannelMaster>().HasData(
                new NotificationChannelMaster
                {
                    Id = (int)NotificationChannelEnum.Email,
                    Name = NotificationChannelEnum.Email,
                    Description = "Email notifications",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationChannelMaster
                {
                    Id = (int)NotificationChannelEnum.SMS,
                    Name = NotificationChannelEnum.SMS,
                    Description = "SMS notifications",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationChannelMaster
                {
                    Id = (int)NotificationChannelEnum.InApp,
                    Name = NotificationChannelEnum.InApp,
                    Description = "In-App notifications",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationChannelMaster
                {
                    Id = (int)NotificationChannelEnum.None,
                    Name = NotificationChannelEnum.None,
                    Description = "No channel selected",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                }
            );

            // Notification Statuses
            modelBuilder.Entity<NotificationStatusMaster>().HasData(
                new NotificationStatusMaster
                {
                    Id = (int)NotificationStatusEnum.Pending,
                    Name = NotificationStatusEnum.Pending,
                    Description = "Notification is pending",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationStatusMaster
                {
                    Id = (int)NotificationStatusEnum.Sent,
                    Name = NotificationStatusEnum.Sent,
                    Description = "Notification sent successfully",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationStatusMaster
                {
                    Id = (int)NotificationStatusEnum.Failed,
                    Name = NotificationStatusEnum.Failed,
                    Description = "Notification failed to deliver",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                }
            );

            // Notification Types
            modelBuilder.Entity<NotificationTypeMaster>().HasData(
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.OrderPlaced,
                    Name = NotificationTypeEnum.OrderPlaced,
                    Description = "Order placed confirmation",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.PaymentSuccess,
                    Name = NotificationTypeEnum.PaymentSuccess,
                    Description = "Payment successful confirmation",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.PaymentFailed,
                    Name = NotificationTypeEnum.PaymentFailed,
                    Description = "Payment failure alert",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.OrderShipped,
                    Name = NotificationTypeEnum.OrderShipped,
                    Description = "Order shipment update",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.OrderDelivered,
                    Name = NotificationTypeEnum.OrderDelivered,
                    Description = "Order delivery confirmation",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.OrderCancelled,
                    Name = NotificationTypeEnum.OrderCancelled,
                    Description = "Order cancellation notification",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTypeMaster
                {
                    Id = (int)NotificationTypeEnum.General,
                    Name = NotificationTypeEnum.General,
                    Description = "General purpose notifications",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                }
            );

            // Notification Templates 
            modelBuilder.Entity<NotificationTemplate>().HasData(
                new NotificationTemplate
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TemplateName = "OrderPlaced_Email",
                    ChannelId = (int)NotificationChannelEnum.Email,
                    TypeId = (int)NotificationTypeEnum.OrderPlaced,
                    SubjectTemplate = "Your Order #{OrderNumber} is Confirmed!",
                    Content = "Hello {CustomerName},<br/>Your order #{OrderNumber} has been placed successfully.<br/>Items: {Items}<br/>Total Amount: ₹{Amount}.<br/>We will notify you once it is shipped.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                    TemplateName = "OrderPlaced_SMS",
                    ChannelId = (int)NotificationChannelEnum.SMS,
                    TypeId = (int)NotificationTypeEnum.OrderPlaced,
                    SubjectTemplate = "Order Confirmation",
                    Content = "Hi {CustomerName}, your order #{OrderNumber} of ₹{Amount} is placed successfully.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
                    TemplateName = "PaymentSuccess_Email",
                    ChannelId = (int)NotificationChannelEnum.Email,
                    TypeId = (int)NotificationTypeEnum.PaymentSuccess,
                    SubjectTemplate = "Payment Received for Order #{OrderNumber}",
                    Content = "Dear {CustomerName},<br/>We have received your payment of ₹{Amount} for order #{OrderNumber}.<br/>Thank you for shopping with us.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    TemplateName = "PaymentFailed_SMS",
                    ChannelId = (int)NotificationChannelEnum.SMS,
                    TypeId = (int)NotificationTypeEnum.PaymentFailed,
                    SubjectTemplate = "Payment Failed",
                    Content = "Hi {CustomerName}, your payment of ₹{Amount} for order #{OrderNumber} has failed. Please retry.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                    TemplateName = "OrderShipped_Email",
                    ChannelId = (int)NotificationChannelEnum.Email,
                    TypeId = (int)NotificationTypeEnum.OrderShipped,
                    SubjectTemplate = "Your Order #{OrderNumber} has been Shipped!",
                    Content = "Hello {CustomerName},<br/>Your order #{OrderNumber} has been shipped.<br/>Track your order here: {TrackingLink}.<br/>Thank you for shopping with us!",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                    TemplateName = "OrderShipped_SMS",
                    ChannelId = (int)NotificationChannelEnum.SMS,
                    TypeId = (int)NotificationTypeEnum.OrderShipped,
                    SubjectTemplate = "Order Shipped",
                    Content = "Hi {CustomerName}, your order #{OrderNumber} has been shipped. Track here: {TrackingLink}",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    TemplateName = "OrderShipped_InApp",
                    ChannelId = (int)NotificationChannelEnum.InApp,
                    TypeId = (int)NotificationTypeEnum.OrderShipped,
                    SubjectTemplate = "Order Update",
                    Content = "Your order #{OrderNumber} has been shipped. Tap to track.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444441"),
                    TemplateName = "OrderDelivered_Email",
                    ChannelId = (int)NotificationChannelEnum.Email,
                    TypeId = (int)NotificationTypeEnum.OrderDelivered,
                    SubjectTemplate = "Your Order #{OrderNumber} has been Delivered",
                    Content = "Dear {CustomerName},<br/>Your order #{OrderNumber} has been successfully delivered.<br/>We hope you enjoy your purchase!",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444442"),
                    TemplateName = "OrderDelivered_SMS",
                    ChannelId = (int)NotificationChannelEnum.SMS,
                    TypeId = (int)NotificationTypeEnum.OrderDelivered,
                    SubjectTemplate = "Order Delivered",
                    Content = "Hi {CustomerName}, your order #{OrderNumber} has been delivered. Thank you for shopping with us!",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444443"),
                    TemplateName = "OrderDelivered_InApp",
                    ChannelId = (int)NotificationChannelEnum.InApp,
                    TypeId = (int)NotificationTypeEnum.OrderDelivered,
                    SubjectTemplate = "Delivery Update",
                    Content = "Your order #{OrderNumber} has been delivered successfully.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555551"),
                    TemplateName = "OrderCancelled_Email",
                    ChannelId = (int)NotificationChannelEnum.Email,
                    TypeId = (int)NotificationTypeEnum.OrderCancelled,
                    SubjectTemplate = "Your Order #{OrderNumber} has been Cancelled",
                    Content = "Dear {CustomerName},<br/>Your order #{OrderNumber} has been cancelled.<br/>If payment was made, refund of ₹{Amount} will be processed shortly.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555552"),
                    TemplateName = "OrderCancelled_SMS",
                    ChannelId = (int)NotificationChannelEnum.SMS,
                    TypeId = (int)NotificationTypeEnum.OrderCancelled,
                    SubjectTemplate = "Order Cancelled",
                    Content = "Hi {CustomerName}, your order #{OrderNumber} has been cancelled. Refund of ₹{Amount} will be processed shortly.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555553"),
                    TemplateName = "OrderCancelled_InApp",
                    ChannelId = (int)NotificationChannelEnum.InApp,
                    TypeId = (int)NotificationTypeEnum.OrderCancelled,
                    SubjectTemplate = "Order Cancelled",
                    Content = "Your order #{OrderNumber} has been cancelled. Refund will be processed soon.",
                    Version = 1,
                    IsDefault = true,
                    EffectiveFrom = createdAt,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = createdAt
                }
            );
        }
    }
}

