using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; } = null!;
    }
}
