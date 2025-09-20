using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        public string EmailOrUserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "ClientId is required.")]
        public string ClientId { get; set; } = null!;
    }
}
