using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Common
{
    public class PaginationDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;
    }
}
