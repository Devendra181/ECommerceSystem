using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.MasterEntities;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        // DbSets for master entities
        public DbSet<EnvironmentMaster> Environments { get; set; }
        public DbSet<GatewayProviderMaster> GatewayProviders { get; set; }
        public DbSet<PaymentMethodTypeMaster> PaymentMethodTypes { get; set; }
        public DbSet<RefundMethodTypeMaster> RefundMethodTypes { get; set; }
        public DbSet<PaymentStatusMaster> PaymentStatuses { get; set; }
        public DbSet<TransactionStatusMaster> TransactionStatuses { get; set; }
        public DbSet<RefundStatusMaster> RefundStatuses { get; set; }
        public DbSet<PaymentProviderConfigurationMaster> PaymentProviderConfigurations { get; set; }

        // DbSets for transactional entities
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<GatewayResponse> GatewayResponses { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relationships & constraints
            // Payment (masters)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany()
                .HasForeignKey(p => p.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.UserPaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.UserPaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethodTypeMaster)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Payment)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.TransactionStatus)
                .WithMany()
                .HasForeignKey(t => t.TransactionStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.PaymentProviderConfigurationMaster)
                .WithMany()
                .HasForeignKey(t => t.PaymentProviderConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            // GatewayResponse 
            modelBuilder.Entity<GatewayResponse>()
                .HasOne(gr => gr.Payment)
                .WithMany(p => p.GatewayResponses)
                .HasForeignKey(gr => gr.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GatewayResponse>()
                .HasOne(gr => gr.Transaction)
                .WithMany(t => t.GatewayResponses)
                .HasForeignKey(gr => gr.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GatewayResponse>()
               .HasOne(gr => gr.Refund)
               .WithMany(r => r.GatewayResponses)
               .HasForeignKey(gr => gr.RefundId)
               .OnDelete(DeleteBehavior.Restrict);

            // Refund
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Payment)
                .WithMany(p => p.Refunds)
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
               .HasOne(p => p.RefundMethodTypeMaster)
               .WithMany()
               .HasForeignKey(p => p.RefundMethodTypeId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Transaction)
                .WithMany()
                .HasForeignKey(r => r.PaymentTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.RefundTransaction)
                .WithMany()
                .HasForeignKey(r => r.RefundTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.RefundStatus)
                .WithMany()
                .HasForeignKey(r => r.RefundStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserPaymentMethod
            modelBuilder.Entity<UserPaymentMethod>()
                .HasOne(upm => upm.MethodType)
                .WithMany()
                .HasForeignKey(upm => upm.MethodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Provider configuration masters
            modelBuilder.Entity<PaymentProviderConfigurationMaster>()
                .HasOne(pc => pc.GatewayProvider)
                .WithMany()
                .HasForeignKey(pc => pc.GatewayProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentProviderConfigurationMaster>()
                .HasOne(pc => pc.Environment)
                .WithMany()
                .HasForeignKey(pc => pc.EnvironmentId)
                .OnDelete(DeleteBehavior.Restrict);

            //Property Conversion
            modelBuilder.Entity<EnvironmentMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<GatewayProviderMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<PaymentMethodTypeMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<PaymentStatusMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<TransactionStatusMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            modelBuilder.Entity<RefundMethodTypeMaster>()
              .Property(c => c.Name)
              .HasConversion<string>();

            modelBuilder.Entity<RefundStatusMaster>()
               .Property(c => c.Name)
               .HasConversion<string>();

            var seedDate = new DateTime(2025, 8, 6, 0, 0, 0, DateTimeKind.Utc);

            // Seed EnvironmentMaster
            modelBuilder.Entity<EnvironmentMaster>().HasData(
                new EnvironmentMaster { Id = 1, Name = EnvironmentTypeEnum.Sandbox, Description = "Sandbox environment for testing", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new EnvironmentMaster { Id = 2, Name = EnvironmentTypeEnum.Live, Description = "Production live environment", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new EnvironmentMaster { Id = 3, Name = EnvironmentTypeEnum.UAT, Description = "User Acceptance testing environment", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed PaymentMethodTypeMaster
            modelBuilder.Entity<PaymentMethodTypeMaster>().HasData(
                new PaymentMethodTypeMaster { Id = 1, Name = PaymentMethodTypeEnum.CreditCard, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentMethodTypeMaster { Id = 2, Name = PaymentMethodTypeEnum.DebitCard, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentMethodTypeMaster { Id = 3, Name = PaymentMethodTypeEnum.UPI, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentMethodTypeMaster { Id = 4, Name = PaymentMethodTypeEnum.Wallet, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentMethodTypeMaster { Id = 5, Name = PaymentMethodTypeEnum.COD, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentMethodTypeMaster { Id = 6, Name = PaymentMethodTypeEnum.NetBanking, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed PaymentMethodTypeMaster
            modelBuilder.Entity<RefundMethodTypeMaster>().HasData(
                new RefundMethodTypeMaster { Id = 1, Name = RefundMethodTypeEnum.Original, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundMethodTypeMaster { Id = 2, Name = RefundMethodTypeEnum.Wallet, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundMethodTypeMaster { Id = 3, Name = RefundMethodTypeEnum.BankTransfer, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundMethodTypeMaster { Id = 4, Name = RefundMethodTypeEnum.Manual, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed PaymentStatusMaster
            modelBuilder.Entity<PaymentStatusMaster>().HasData(
                new PaymentStatusMaster { Id = 1, Name = PaymentStatusEnum.Pending, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentStatusMaster { Id = 2, Name = PaymentStatusEnum.Completed, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentStatusMaster { Id = 3, Name = PaymentStatusEnum.Failed, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentStatusMaster { Id = 4, Name = PaymentStatusEnum.Canceled, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed TransactionStatusMaster
            modelBuilder.Entity<TransactionStatusMaster>().HasData(
                new TransactionStatusMaster { Id = 1, Name = TransactionStatusEnum.Pending, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new TransactionStatusMaster { Id = 2, Name = TransactionStatusEnum.Success, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new TransactionStatusMaster { Id = 3, Name = TransactionStatusEnum.Failed, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new TransactionStatusMaster { Id = 4, Name = TransactionStatusEnum.Canceled, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed RefundStatusMaster
            modelBuilder.Entity<RefundStatusMaster>().HasData(
                new RefundStatusMaster { Id = 1, Name = RefundStatusEnum.Pending, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundStatusMaster { Id = 2, Name = RefundStatusEnum.Completed, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundStatusMaster { Id = 3, Name = RefundStatusEnum.Failed, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new RefundStatusMaster { Id = 4, Name = RefundStatusEnum.Rejected, IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed GatewayProviderMaster
            modelBuilder.Entity<GatewayProviderMaster>().HasData(
               new GatewayProviderMaster { Id = 1, Name = GatewayProviderEnum.Razorpay, Description = "Razorpay payment gateway", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
               new GatewayProviderMaster { Id = 2, Name = GatewayProviderEnum.Stripe, Description = "Stripe payment gateway", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
               new GatewayProviderMaster { Id = 3, Name = GatewayProviderEnum.Paytm, Description = "Paytm payment gateway", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
               new GatewayProviderMaster { Id = 4, Name = GatewayProviderEnum.Other, Description = "Other payment gateway", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });

            // Seed PaymentProviderConfiguration
            modelBuilder.Entity<PaymentProviderConfigurationMaster>().HasData(
                new PaymentProviderConfigurationMaster { Id = 1, GatewayProviderId = 1, EnvironmentId = 1, ApiKey = "rzp_test_sandbox_key", ApiSecret = "rzp_test_sandbox_secret", EndpointUrl = "https://api.razorpay.com/v1/", WebhookUrl = "https://yourdomain.com/api/webhook/razorpay", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentProviderConfigurationMaster { Id = 2, GatewayProviderId = 1, EnvironmentId = 2, ApiKey = "rzp_live_production_key", ApiSecret = "rzp_live_production_secret", EndpointUrl = "https://api.razorpay.com/v1/", WebhookUrl = "https://yourdomain.com/api/webhook/razorpay", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentProviderConfigurationMaster { Id = 3, GatewayProviderId = 2, EnvironmentId = 1, ApiKey = "sk_test_sandbox_key", ApiSecret = "sk_test_sandbox_secret", EndpointUrl = "https://api.stripe.com/v1/", WebhookUrl = "https://yourdomain.com/api/webhook/stripe", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentProviderConfigurationMaster { Id = 4, GatewayProviderId = 2, EnvironmentId = 2, ApiKey = "sk_live_production_key", ApiSecret = "sk_live_production_secret", EndpointUrl = "https://api.stripe.com/v1/", WebhookUrl = "https://yourdomain.com/api/webhook/stripe", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentProviderConfigurationMaster { Id = 5, GatewayProviderId = 3, EnvironmentId = 1, ApiKey = "paytm_test_sandbox_key", ApiSecret = "paytm_test_sandbox_secret", EndpointUrl = "https://secure.paytm.in/", WebhookUrl = "https://yourdomain.com/api/webhook/paytm", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate },
                new PaymentProviderConfigurationMaster { Id = 6, GatewayProviderId = 3, EnvironmentId = 2, ApiKey = "paytm_live_production_key", ApiSecret = "paytm_live_production_secret", EndpointUrl = "https://secure.paytm.in/", WebhookUrl = "https://yourdomain.com/api/webhook/paytm", IsActive = true, CreatedOn = seedDate, UpdatedOn = seedDate });
        }
    }
}
