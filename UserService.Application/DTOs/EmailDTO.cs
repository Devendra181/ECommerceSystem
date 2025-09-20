using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs
{
    public class EmailDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = null!;
    }
}
