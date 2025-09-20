namespace UserService.Application.DTOs
{
    public class RefreshTokenResponseDTO
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
