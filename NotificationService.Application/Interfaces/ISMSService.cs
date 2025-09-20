namespace NotificationService.Application.Interfaces
{
    public interface ISMSService
    {
        Task<bool> SendSmsAsync(string toPhoneNumber, string message);
    }
}
