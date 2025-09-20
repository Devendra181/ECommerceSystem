using System.ComponentModel.DataAnnotations;
namespace PaymentService.Application.DTOs
{
    public class PaymentStatusRequestDTO
    {
        [Required]
        public Guid PaymentId { get; set; }
    }
}
