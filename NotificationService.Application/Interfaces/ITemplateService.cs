namespace NotificationService.Application.Interfaces
{
    public interface ITemplateService
    {
        Task<(string Subject, string Content)> ResolveAsync(int typeId, int channelId, int? version, Dictionary<string, object>? data);
    }
}
