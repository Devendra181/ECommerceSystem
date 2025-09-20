using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
namespace NotificationService.Application.Services
{
    public class PreferenceService : IPreferenceService
    {
        private readonly IUserPreferenceRepository _preferences;

        public PreferenceService(IUserPreferenceRepository preferences)
        {
            _preferences = preferences;
        }

        public async Task UpsertAsync(PreferenceDTO dto)
        {
            var entity = new UserPreference
            {
                UserId = dto.UserId,
                EmailEnabled = dto.EmailEnabled,
                SmsEnabled = dto.SmsEnabled,
                InAppEnabled = dto.InAppEnabled,
                DoNotDisturb = dto.DoNotDisturb,
                MaxDailyNotifications = dto.MaxDailyNotifications,
                QuietHoursStart = dto.QuietHoursStart,
                QuietHoursEnd = dto.QuietHoursEnd,
                IsActive = true
            };

            await _preferences.UpsertAsync(entity);
        }

        public async Task<bool> IsUserInDndAsync(Guid userId, DateTime utcNow)
        {
            var list = await _preferences.GetAllDoNotDisturbUsersAsync(utcNow);
            foreach (var up in list)
            {
                if (up.UserId == userId) return true;
            }
            return false;
        }
    }
}
