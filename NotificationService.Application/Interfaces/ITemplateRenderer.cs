namespace NotificationService.Application.Interfaces
{
    // Renders tokenized subject/content with data e.g., {OrderNumber}
    public interface ITemplateRenderer
    {
        string Render(string template, IReadOnlyDictionary<string, object>? data);
    }
}
