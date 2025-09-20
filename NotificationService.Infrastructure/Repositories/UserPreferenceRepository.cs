using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace NotificationService.Infrastructure.Repositories
{
    public class UserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly NotificationDbContext _context;

        public UserPreferenceRepository(NotificationDbContext context)
        {
            _context = context;
        }

        // READ
        public async Task<UserPreference?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.IsActive);
        }

        // UPSERT (by UserId)
        public async Task<UserPreference> UpsertAsync(UserPreference preference)
        {
            if (preference == null) throw new ArgumentNullException(nameof(preference));

            var existing = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == preference.UserId);

            if (existing == null)
            {
                // Insert new record
                preference.CreatedAt = DateTime.UtcNow;
                preference.CreatedBy = "System";
                preference.IsActive = true;
                await _context.UserPreferences.AddAsync(preference);
            }
            else
            {
                // Update existing
                existing.EmailEnabled = preference.EmailEnabled;
                existing.SmsEnabled = preference.SmsEnabled;
                existing.InAppEnabled = preference.InAppEnabled;
                existing.DoNotDisturb = preference.DoNotDisturb;
                existing.MaxDailyNotifications = preference.MaxDailyNotifications;
                existing.QuietHoursStart = preference.QuietHoursStart;
                existing.QuietHoursEnd = preference.QuietHoursEnd;

                // Reactivate if it had been soft-deleted earlier
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";

                _context.UserPreferences.Update(existing);
            }

            await _context.SaveChangesAsync();
            return preference;
        }

        // SOFT DELETE (sets IsActive = false)
        public async Task DeleteAsync(Guid id)
        {
            var preference = await _context.UserPreferences.FindAsync(id);
            if (preference != null)
            {
                preference.IsActive = false;
                preference.UpdatedAt = DateTime.UtcNow;
                preference.UpdatedBy = "System";
                _context.UserPreferences.Update(preference);
                await _context.SaveChangesAsync();
            }
        }

        // Reporting: All users currently in Do-Not-Disturb mode (DND=true OR now within quiet hours).
        // Quiet-hours logic handles ranges that cross midnight (e.g., 22:00–06:00).
        public async Task<IReadOnlyList<UserPreference>> GetAllDoNotDisturbUsersAsync(DateTime utcNow)
        {
            var currentTime = utcNow.TimeOfDay;

            return await _context.UserPreferences
                .AsNoTracking()
                .Where(up => up.IsActive && (
                        up.DoNotDisturb
                        ||
                        (
                            up.QuietHoursStart.HasValue && up.QuietHoursEnd.HasValue
                            &&
                            (
                                // Normal window (start <= end): start <= now < end
                                (up.QuietHoursStart <= up.QuietHoursEnd
                                    && up.QuietHoursStart <= currentTime
                                    && currentTime < up.QuietHoursEnd)
                                ||
                                // Overnight window (start > end): now >= start OR now < end
                                (up.QuietHoursStart > up.QuietHoursEnd
                                    && (currentTime >= up.QuietHoursStart || currentTime < up.QuietHoursEnd))
                            )
                        )
                    ))
                .ToListAsync();
        }
    }
}

