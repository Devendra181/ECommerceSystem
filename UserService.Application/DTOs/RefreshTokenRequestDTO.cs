using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;

        [Required(ErrorMessage = "Client ID is required.")]
        public string ClientId { get; set; } = null!; // e.g., "Web", "Android", "iOS"
    }
}
