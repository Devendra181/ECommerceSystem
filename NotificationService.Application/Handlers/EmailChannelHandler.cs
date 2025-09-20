using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Handlers
{
    public class EmailChannelHandler : INotificationChannelHandler
    {
        private readonly IEmailService _emailService;

        public EmailChannelHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public NotificationChannelEnum Channel => NotificationChannelEnum.Email;

        public async Task<(bool Success, string? ProviderMessage, string? Error)> SendAsync(Notification n)
        {
            try
            {
                var toEmails = n.Recipients
                    .Where(r => !string.IsNullOrWhiteSpace(r.Email)
                            && r.RecipientType == RecipientTypeEnum.To)
                    .Select(r => r.Email!)
                    .Distinct()
                    .ToList();

                if (!toEmails.Any())
                    return (false, null, "Email channel requires at least one recipient.");

                var ccEmails = n.Recipients
                    .Where(r => !string.IsNullOrWhiteSpace(r.Email)
                            && r.RecipientType == RecipientTypeEnum.CC)
                    .Select(r => r.Email!)
                    .ToList();

                var bccEmails = n.Recipients
                    .Where(r => !string.IsNullOrWhiteSpace(r.Email)
                            && r.RecipientType == RecipientTypeEnum.BCC)
                    .Select(r => r.Email!)
                    .ToList();

                var attachments = n.Attachments?.Select(a => new AttachmentDTO
                {
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    MimeType = a.MimeType
                }).ToList();

                await _emailService.SendEmailAsync(
                    subject: n.Subject,
                    body: n.TemplateData,
                    isBodyHtml: true,
                    toEmail: null,
                    toEmails: toEmails,
                    ccEmails: ccEmails,
                    bccEmails: bccEmails,
                    attachments: attachments);

                return (true, "Email dispatched", null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}

