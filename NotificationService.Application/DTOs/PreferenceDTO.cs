namespace NotificationService.Application.DTOs
{
    public class PreferenceDTO
    {
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
        public bool DoNotDisturb { get; set; } = false;
        public int? MaxDailyNotifications { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}
