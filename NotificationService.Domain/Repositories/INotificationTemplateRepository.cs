using NotificationService.Domain.Entities;
namespace NotificationService.Domain.Repositories
{
    public interface INotificationTemplateRepository
    {
        Task<NotificationTemplate?> GetByIdAsync(Guid id);
        Task<NotificationTemplate?> GetActiveTemplateAsync(
            int typeId, int channelId, DateTime onUtc,
            int? version = null);
        Task<IReadOnlyList<NotificationTemplate>> GetAllActiveByTypeAndChannelAsync(
            int typeId, int channelId);
        Task<IReadOnlyList<NotificationTemplate>> GetAllAsync();
    }
}
