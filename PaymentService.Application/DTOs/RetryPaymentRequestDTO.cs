using PaymentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace PaymentService.Application.DTOs
{
    public class RetryPaymentRequestDTO
    {
        [Required]
        public Guid PaymentId { get; set; }
        public PaymentMethodTypeEnum? PaymentMethodOverride { get; set; }
    }
}
