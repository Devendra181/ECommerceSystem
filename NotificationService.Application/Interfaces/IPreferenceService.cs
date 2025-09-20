using NotificationService.Application.DTOs;
namespace NotificationService.Application.Interfaces
{
    public interface IPreferenceService
    {
        Task UpsertAsync(PreferenceDTO dto);
        Task<bool> IsUserInDndAsync(Guid userId, DateTime utcNow);
    }
}
