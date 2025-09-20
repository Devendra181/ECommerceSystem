using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Persistence;
namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationTemplateRepository(NotificationDbContext context)
        {
            _context = context;
        }

        // === Get by Id ===
        public async Task<NotificationTemplate?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationTemplates
                .AsNoTracking()
                .Include(t => t.AuditTrail)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        // === Get active template (with optional version) ===
        public async Task<NotificationTemplate?> GetActiveTemplateAsync(
            int typeId, int channelId, DateTime onUtc, int? version = null)
        {
            var query = _context.NotificationTemplates.AsNoTracking().AsQueryable();

            query = query.Where(t =>
                t.TypeId == typeId &&
                t.ChannelId == channelId &&
                t.IsActive &&
                t.EffectiveFrom <= onUtc &&
                (t.EffectiveTo == null || t.EffectiveTo >= onUtc));

            if (version.HasValue)
            {
                // Exact version match
                query = query.Where(t => t.Version == version.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        // === Get all active templates for a given type/channel ===
        public async Task<IReadOnlyList<NotificationTemplate>> GetAllActiveByTypeAndChannelAsync(
            int typeId, int channelId)
        {
            return await _context.NotificationTemplates
                .AsNoTracking()
                .Where(t =>
                    t.TypeId == typeId &&
                    t.ChannelId == channelId &&
                    t.IsActive)
                .OrderByDescending(t => t.Version)
                .ThenByDescending(t => t.EffectiveFrom)
                .ToListAsync();
        }

        // === Get all templates (admin/debug) ===
        public async Task<IReadOnlyList<NotificationTemplate>> GetAllAsync()
        {
            return await _context.NotificationTemplates
                .AsNoTracking()
                .Include(t => t.AuditTrail)
                .OrderBy(t => t.TemplateName)
                .ThenBy(t => t.ChannelId)
                .ThenByDescending(t => t.Version)
                .ToListAsync();
        }
    }
}
