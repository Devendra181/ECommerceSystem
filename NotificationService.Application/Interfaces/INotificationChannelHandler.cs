using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
namespace NotificationService.Application.Interfaces
{
    public interface INotificationChannelHandler
    {
        NotificationChannelEnum Channel { get; }
        Task<(bool Success, string? ProviderMessage, string? Error)> SendAsync(Notification notification);
    }
}
