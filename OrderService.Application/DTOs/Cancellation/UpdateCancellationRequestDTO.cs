using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Cancellation
{
    public class UpdateCancellationRequestDTO
    {
        [Required]
        public Guid CancellationId { get; set; }  // Cancellation Id to update

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Cancellation reason is required.")]
        public int ReasonId { get; set; }  // Cancellation reason Id
        public string? Remarks { get; set; }
        public bool IsPartial { get; set; }  // Partial or full cancellation

        // List of cancellation items (order items being cancelled with quantities)
        public List<UpdateCancellationItemDTO>? CancellationItems { get; set; }
    }
}

