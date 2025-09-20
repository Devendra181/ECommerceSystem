namespace UserService.Application.DTOs
{
    public class LoginResponseDTO
    {
        public bool Succeeded { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string? ErrorMessage { get; set; }
        public int? RemainingAttempts { get; set; }
    }
}
