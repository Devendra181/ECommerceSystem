using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cancellation
{
    public class CreateCancellationRequestDTO
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "ReasonId is required.")]
        public int ReasonId { get; set; }

        [MaxLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        public bool IsPartial { get; set; }

        public List<CancellationItemRequestDTO>? CancellationItems { get; set; }
    }
}
