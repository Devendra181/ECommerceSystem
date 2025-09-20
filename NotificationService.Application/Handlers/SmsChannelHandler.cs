using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Application.Handlers
{
    public class SmsChannelHandler : INotificationChannelHandler
    {
        private readonly ISMSService _smsService;

        public SmsChannelHandler(ISMSService smsService)
        {
            _smsService = smsService;
        }

        public NotificationChannelEnum Channel => NotificationChannelEnum.SMS;

        public async Task<(bool Success, string? ProviderMessage, string? Error)> SendAsync(Notification n)
        {
            try
            {
                var toPhoneNumber = n.Recipients.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.PhoneNumber))?.PhoneNumber;

                if (toPhoneNumber is null)
                    throw new ValidationException("SMS channel requires at least one recipient.");

                var success = await _smsService.SendSmsAsync(toPhoneNumber!, n.TemplateData);

                return success
                    ? (true, "SMS accepted by provider.", null)
                    : (false, null, "SMS not accepted by provider.");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
