using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
namespace NotificationService.Application.Handlers
{
    public class InAppChannelHandler : INotificationChannelHandler
    {
        public NotificationChannelEnum Channel => NotificationChannelEnum.InApp;

        public async Task<(bool Success, string? ProviderMessage, string? Error)> SendAsync(Notification n)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            // InApp notifications are stored only (no external provider)
            return (true, "Stored as InApp notification", null);
        }
    }
}
