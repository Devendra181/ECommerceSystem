using NotificationService.Application.DTOs;
namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Guid> CreateAsync(CreateNotificationRequestDTO request);
        Task ProcessQueueBatchAsync(int take, int skip);
        Task<List<NotificationResponseDTO>> GetUserAsync(Guid userId, int take = 50, int skip = 0);
        Task DisableAsync(Guid id);
    }
}
