using NotificationService.Application.DTOs;
namespace NotificationService.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string subject,
            string body,
            bool isBodyHtml = false,
            string? toEmail = null,
            IEnumerable<string>? toEmails = null,
            IEnumerable<string>? ccEmails = null,
            IEnumerable<string>? bccEmails = null,
            IEnumerable<AttachmentDTO>? attachments = null,
            string? senderDisplayName = null);
    }
}
