using NotificationService.Domain.Entities;
namespace NotificationService.Domain.Repositories
{
    public interface INotificationRepository
    {
        // Create
        Task<Notification> CreateAsync(Notification notification);

        // Read
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, int take = 50, int skip = 0);
        Task<IReadOnlyList<Notification>> GetQueueBatchAsync(int take, int skip, DateTime utcNow); // Pending 
        Task<IReadOnlyList<Notification>> GetFailedAsync(DateTime fromUtc, DateTime toUtc);

        // Update status & retry
        Task UpdateStatusAsync(Guid id, int statusId, string? errorMessage = null);
        Task IncrementRetryAsync(Guid id);

        // Delete (Soft delete - sets IsActive=false)
        Task DeleteAsync(Guid id);

        // Child aggregates
        Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients);
        Task AddAttachmentsAsync(IEnumerable<NotificationAttachment> attachments);

        // Attempts
        Task AddAttemptAsync(NotificationAttemptLog attempt);
    }
}
