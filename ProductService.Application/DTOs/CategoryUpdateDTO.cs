using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    // For Update - Id required
    public class CategoryUpdateDTO
    {
        [Required(ErrorMessage = "Category Id is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Category Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
