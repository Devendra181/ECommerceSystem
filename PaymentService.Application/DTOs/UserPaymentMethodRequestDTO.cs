using PaymentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.DTOs
{
    // DTO to add or update user payment method
    public class UserPaymentMethodRequestDTO
    {
        public Guid UserId { get; set; }
        public PaymentMethodTypeEnum PaymentMethodType { get; set; }
        public string MaskedDetails { get; set; } = null!;

        [Range(1, 12)]
        public int? ExpiryMonth { get; set; }

        [Range(2000, 2100)]
        public int? ExpiryYear { get; set; }
    }
}
