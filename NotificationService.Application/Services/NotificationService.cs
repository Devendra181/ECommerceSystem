using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifications;
        private readonly IUserPreferenceRepository _preferences;
        private readonly ITemplateService _templateService;
        private readonly IEnumerable<INotificationChannelHandler> _channelHandlers;
        private readonly ILogger<NotificationService> _logger;

        private const int MaxRetries = 3;

        public NotificationService(
            INotificationRepository notifications,
            IUserPreferenceRepository preferences,
            ITemplateService templateService,
            IEnumerable<INotificationChannelHandler> channelHandlers,
            ILogger<NotificationService> logger)
        {
            _notifications = notifications;
            _preferences = preferences;
            _templateService = templateService;
            _channelHandlers = channelHandlers;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(CreateNotificationRequestDTO request)
        {
            ValidateCreate(request);

            // enforce daily limit
            var pref = await _preferences.GetByUserIdAsync(request.UserId);
            if (pref?.MaxDailyNotifications is int maxPerDay)
            {
                var today = DateTime.UtcNow.Date;
                var sentToday = (await _notifications.GetByUserAsync(request.UserId, 500, 0))
                                    .Count(n => n.CreatedAt.Date == today && n.StatusId == (int)NotificationStatusEnum.Sent);

                if (sentToday >= maxPerDay)
                    throw new ValidationException("User has reached daily notification limit.");
            }

            // resolve subject & content 
            var resolved = await _templateService.ResolveAsync(
                    request.TypeId, (int)request.Channel, request.TemplateVersion, request.TemplateData);

            string subject = resolved.Subject;
            string content = resolved.Content;

            var entity = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ChannelId = (int)request.Channel,
                TypeId = request.TypeId,
                Subject = subject,
                StatusId = (int)NotificationStatusEnum.Pending,
                TemplateData = content,
                RetryCount = 0,
                ScheduledAt = request.ScheduledAtUtc,
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            var created = await _notifications.CreateAsync(entity);
            await _notifications.AddRecipientsAsync(request.Recipients.Select(r => new NotificationRecipient
            {
                Id = Guid.NewGuid(),
                NotificationId = created.Id,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                RecipientType = (RecipientTypeEnum)r.RecipientTypeId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            }));

            if (request.Attachments?.Any() == true)
            {
                await _notifications.AddAttachmentsAsync(request.Attachments.Select(a => new NotificationAttachment
                {
                    Id = Guid.NewGuid(),
                    NotificationId = created.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    MimeType = a.MimeType,
                    FileSize = a.FileSize,
                    StorageType = a.StorageType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                }));
            }

            _logger.LogInformation("Notification {NotificationId} created for User {UserId}", created.Id, created.UserId);
            return created.Id;
        }

        public async Task ProcessQueueBatchAsync(int take, int skip)
        {
            // Fetch due notifications
            var due = await _notifications.GetQueueBatchAsync(take, skip, DateTime.UtcNow);

            if (due == null || due.Count == 0)
            {
                _logger.LogInformation("No notifications due for processing.");
                return;
            }

            foreach (var notification in due)
            {
                try
                {
                    await ProcessSingleAsync(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Notification {Id}", notification.Id);
                    await _notifications.UpdateStatusAsync(notification.Id,
                        (int)NotificationStatusEnum.Failed, ex.Message);
                }
            }
        }

        private async Task ProcessSingleAsync(Notification notification)
        {
            try
            {
                if (await IsUserInDndAsync(notification.UserId, DateTime.UtcNow))
                {
                    var pref = await _preferences.GetByUserIdAsync(notification.UserId);
                    notification.ScheduledAt = pref?.QuietHoursEnd.HasValue == true
                        ? DateTime.UtcNow.Date.Add(pref.QuietHoursEnd.Value)   // schedule at quiet hours end today
                        : DateTime.UtcNow.AddMinutes(15);                     // fallback
                    await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Pending, "Deferred due to DND/QuietHours");
                    return;
                }

                if (notification.RetryCount >= MaxRetries)
                {
                    await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Failed, "Max retries exceeded");
                    return;
                }

                var handler = _channelHandlers.FirstOrDefault(h => (int)h.Channel == notification.ChannelId);
                if (handler == null)
                {
                    await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Failed, "No handler for channel");
                    return;
                }

                var (success, providerMsg, error) = await handler.SendAsync(notification);

                var attempt = new NotificationAttemptLog
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    AttemptNumber = notification.RetryCount + 1,
                    AttemptedAt = DateTime.UtcNow,
                    StatusId = success ? (int)NotificationStatusEnum.Sent : (int)NotificationStatusEnum.Failed,
                    ChannelId = notification.ChannelId,
                    ProviderResponse = providerMsg,
                    IsSuccessful = success,
                    ErrorMessage = error
                };
                await _notifications.AddAttemptAsync(attempt);

                if (success)
                    await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Sent, providerMsg);
                else
                {
                    await _notifications.IncrementRetryAsync(notification.Id);
                    await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Pending, error ?? "Will retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {Id}", notification.Id);
                await _notifications.UpdateStatusAsync(notification.Id, (int)NotificationStatusEnum.Failed, ex.Message);
            }
        }

        public async Task<List<NotificationResponseDTO>> GetUserAsync(Guid userId, int take = 50, int skip = 0)
        {
            var list = await _notifications.GetByUserAsync(userId, take, skip);
            return list.Select(n => new NotificationResponseDTO
            {
                Id = n.Id,
                UserId = n.UserId,
                ChannelId = n.ChannelId,
                TypeId = n.TypeId,
                Subject = n.Subject,
                Message = n.TemplateData,
                StatusId = n.StatusId,
                RetryCount = n.RetryCount,
                ScheduledAt = n.ScheduledAt,
                Priority = n.Priority,
                ErrorMessage = n.ErrorMessage,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task DisableAsync(Guid id)
        {
            await _notifications.DeleteAsync(id);
        }

        #region Private Methods
        private static void ValidateCreate(CreateNotificationRequestDTO request)
        {
            var errors = new List<string>();

            if (request.UserId == Guid.Empty)
                errors.Add("UserId is required.");

            if (!request.Recipients.Any())
                errors.Add("At least one recipient is required.");

            if (request.Channel == NotificationChannelEnum.Email &&
                !request.Recipients.Any(r => !string.IsNullOrWhiteSpace(r.Email)))
                errors.Add("Email channel requires at least one Email recipient.");

            if (request.Channel == NotificationChannelEnum.SMS &&
                !request.Recipients.Any(r => !string.IsNullOrWhiteSpace(r.PhoneNumber)))
                errors.Add("SMS channel requires at least one PhoneNumber recipient.");

            if (errors.Count > 0)
                throw new ValidationException(string.Join(" ", errors));
        }

        private async Task<bool> IsUserInDndAsync(Guid userId, DateTime nowUtc)
        {
            var pref = await _preferences.GetByUserIdAsync(userId);
            if (pref is null || !pref.IsActive)
                return false;

            if (pref.DoNotDisturb)
                return true;

            if (pref.QuietHoursStart.HasValue && pref.QuietHoursEnd.HasValue)
            {
                var now = nowUtc.TimeOfDay;
                var start = pref.QuietHoursStart.Value;
                var end = pref.QuietHoursEnd.Value;

                if (start <= end)
                {
                    if (now >= start && now < end) return true;
                }
                else
                {
                    if (now >= start || now < end) return true;
                }
            }

            return false;
        }

        #endregion
    }
}
