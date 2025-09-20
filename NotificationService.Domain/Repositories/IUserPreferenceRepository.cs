using NotificationService.Domain.Entities;
namespace NotificationService.Domain.Repositories
{
    public interface IUserPreferenceRepository
    {
        // Read
        Task<UserPreference?> GetByUserIdAsync(Guid userId);

        // Upsert
        Task<UserPreference> UpsertAsync(UserPreference preference);

        // Delete (Soft delete - sets IsActive=false)
        Task DeleteAsync(Guid id);

        // Reporting
        Task<IReadOnlyList<UserPreference>> GetAllDoNotDisturbUsersAsync(DateTime utcNow);
    }
}
