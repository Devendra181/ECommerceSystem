using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public DateTime GeneratedAt { get; set; }

        [Required, MaxLength(100)]
        public string InvoiceNumber { get; set; } = null!;

        [Required, MaxLength(1000)]
        public string InvoicePdfUrl { get; set; } = null!;
    }
}
