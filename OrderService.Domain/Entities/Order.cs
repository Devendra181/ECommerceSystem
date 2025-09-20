using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    [Index(nameof(UserId))]
    [Index(nameof(OrderStatusId))]
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string OrderNumber { get; set; } = null!;
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotalAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCharges { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;  
        [Required]
        public int OrderStatusId { get; set; }
        [ForeignKey(nameof(OrderStatusId))]
        public OrderStatus? OrderStatus { get; set; }
        [Required]
        public Guid ShippingAddressId { get; set; }
        [Required]
        public Guid BillingAddressId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? CancellationPolicyId { get; set; }
        [ForeignKey(nameof(CancellationPolicyId))]
        public CancellationPolicy? CancellationPolicy { get; set; }
        public int? ReturnPolicyId { get; set; }
        [ForeignKey(nameof(ReturnPolicyId))]
        public ReturnPolicy? ReturnPolicy { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Cancellation> OrderCancellations { get; set; } = new List<Cancellation>();
        public ICollection<Return> OrderReturns { get; set; } = new List<Return>();
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public ICollection<Shipment> Logistics { get; set; } = new List<Shipment>();
        public Invoice? Invoice { get; set; }
    }
}
