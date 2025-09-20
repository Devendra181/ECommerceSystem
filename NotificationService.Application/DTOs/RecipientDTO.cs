namespace NotificationService.Application.DTOs
{
    public class RecipientDTO
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int RecipientTypeId { get; set; } // maps to RecipientTypeEnum
    }
}
