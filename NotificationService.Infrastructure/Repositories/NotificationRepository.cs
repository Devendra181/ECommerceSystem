using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Persistence;
namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        // === Create ===
        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        // === Read ===
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.Recipients)
                .Include(n => n.Attachments)
                .Include(n => n.AttemptLogs)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, int take = 50, int skip = 0)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.IsActive == true)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(n => n.Recipients)
                .Include(n => n.Attachments)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Notification>> GetQueueBatchAsync(int take, int skip, DateTime UtcNow)
        {
            // Pending + due now (High = 1, Normal = 2, Low = 3) -> ascending priority puts High first
            return await _context.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.StatusId == (int)NotificationStatusEnum.Pending &&
                    (n.ScheduledAt == null || n.ScheduledAt <= UtcNow) &&
                     n.IsActive == true)
                .OrderBy(n => n.Priority)                     // High(1) first
                .ThenBy(n => n.ScheduledAt ?? n.CreatedAt)    // older first
                .Skip(skip)
                .Take(take)
                .Include(n => n.Recipients)
                .Include(n => n.Attachments)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Notification>> GetFailedAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.StatusId == (int)NotificationStatusEnum.Failed &&
                    n.CreatedAt >= fromUtc &&
                    n.CreatedAt <= toUtc &&
                    n.IsActive == true)
                .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                .ToListAsync();
        }

        // === Update status & retry ===
        public async Task UpdateStatusAsync(Guid id, int statusId, string? errorMessage = null)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.StatusId = statusId;
                notification.ErrorMessage = errorMessage;
                notification.UpdatedAt = DateTime.UtcNow;
                notification.UpdatedBy = "System";
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementRetryAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.RetryCount += 1;
                notification.UpdatedAt = DateTime.UtcNow;
                notification.UpdatedBy = "System";
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }

        // === Delete (Soft delete) ===
        public async Task DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                // Soft delete
                notification.IsActive = false;
                notification.UpdatedAt = DateTime.UtcNow;
                notification.UpdatedBy = "System";
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }

        // === Child aggregates ===
        public async Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients)
        {
            await _context.NotificationRecipients.AddRangeAsync(recipients);
            await _context.SaveChangesAsync();
        }

        public async Task AddAttachmentsAsync(IEnumerable<NotificationAttachment> attachments)
        {
            await _context.NotificationAttachments.AddRangeAsync(attachments);
            await _context.SaveChangesAsync();
        }

        // === Attempts ===
        public async Task AddAttemptAsync(NotificationAttemptLog attempt)
        {
            await _context.NotificationAttemptLogs.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }
    }
}
