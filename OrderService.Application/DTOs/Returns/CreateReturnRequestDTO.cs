using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class CreateReturnRequestDTO
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "ReasonId is required.")]
        public int ReasonId { get; set; }
        public bool IsPartial { get; set; }

        [MaxLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "At least one return item is required.")]
        [MinLength(1, ErrorMessage = "At least one return item is required.")]
        public List<ReturnItemRequestDTO> ReturnItems { get; set; } = new();
    }
}
