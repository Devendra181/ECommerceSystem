using NotificationService.Application.Interfaces;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly INotificationTemplateRepository _templates;
        private readonly ITemplateRenderer _renderer;

        public TemplateService(INotificationTemplateRepository templates, ITemplateRenderer renderer)
        {
            _templates = templates;
            _renderer = renderer;
        }

        public async Task<(string Subject, string Content)> ResolveAsync(
            int typeId,
            int channelId,
            int? version,
            Dictionary<string, object>? data)
        {
            var tpl = await _templates.GetActiveTemplateAsync(typeId, channelId, DateTime.UtcNow, version);
            if (tpl is null)
                throw new Exception($"No active template found for TypeId={typeId}, ChannelId={channelId}.");

            var subject = _renderer.Render(tpl.SubjectTemplate, data);
            var content = _renderer.Render(tpl.Content, data);
            return (subject, content);
        }
    }
}

