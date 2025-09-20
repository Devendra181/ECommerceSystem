using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using NotificationService.Application.DTOs;
namespace NotificationService.Application.Utilities
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string subject,
            string body,
            bool isBodyHtml = false,
            string? toEmail = null,
            IEnumerable<string>? toEmails = null,
            IEnumerable<string>? ccEmails = null,
            IEnumerable<string>? bccEmails = null,
            IEnumerable<AttachmentDTO>? attachments = null,
            string? senderDisplayName = null)
        {
            // Read Config
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var defaultSenderName = _configuration["EmailSettings:SenderName"];
            var password = _configuration["EmailSettings:AppPassword"];

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("EmailSettings are missing: SmtpServer, SenderEmail, and Password are required.");
            }

            // Build unified recipient sets
            var toList = new List<string>();
            if (!string.IsNullOrWhiteSpace(toEmail))
                toList.Add(toEmail);

            if (toEmails != null)
                toList.AddRange(toEmails.Where(e => !string.IsNullOrWhiteSpace(e)));

            var ccList = (ccEmails ?? Enumerable.Empty<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
            var bccList = (bccEmails ?? Enumerable.Empty<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

            toList = toList.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

            if (toList.Count == 0)
                throw new ArgumentException("At least one To recipient must be provided via toEmail or toEmails.");

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, string.IsNullOrWhiteSpace(senderDisplayName) ? defaultSenderName : senderDisplayName),
                Subject = subject ?? string.Empty,
                Body = body ?? string.Empty,
                IsBodyHtml = isBodyHtml
            };

            foreach (var to in toList)
                message.To.Add(new MailAddress(to));

            foreach (var cc in ccList)
                message.CC.Add(new MailAddress(cc));

            foreach (var bcc in bccList)
                message.Bcc.Add(new MailAddress(bcc));

            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(a.FilePath) && File.Exists(a.FilePath))
                        {
                            var att = new Attachment(a.FilePath, a.MimeType);
                            att.Name = string.IsNullOrWhiteSpace(a.FileName) ? Path.GetFileName(a.FilePath) : a.FileName;
                            message.Attachments.Add(att);
                        }
                        else
                        {
                            _logger.LogWarning("Email attachment not found or invalid: {Path}", a.FilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to attach file: {Path}", a.FilePath);
                    }
                }
            }

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}

