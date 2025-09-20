using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class UpdateReturnRequestDTO
    {
        [Required]
        public Guid ReturnId { get; set; } // Return Id to update
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int ReasonId { get; set; } // Return reason Id
        public string? Remarks { get; set; }
        public bool IsPartial { get; set; } // Partial or full Return

        // List of Return items(order items being cancelled with quantities)
        public List<UpdateReturnItemDTO>? ReturnItems { get; set; }
    }
}