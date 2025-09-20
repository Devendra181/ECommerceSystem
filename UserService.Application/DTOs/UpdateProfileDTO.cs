using System.ComponentModel.DataAnnotations;
namespace UserService.Application.DTOs
{
    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!;

        [Url(ErrorMessage = "Profile photo URL must be a valid URL.")]
        public string? ProfilePhotoUrl { get; set; }
    }
}
