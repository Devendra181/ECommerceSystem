namespace UserService.Application.DTOs
{
    public class ForgotPasswordResponseDTO
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
    }
}
