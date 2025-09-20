namespace Messaging.Common.Models
{
    public abstract class EventBase
    {
        //Unique ID per event
        public Guid EventId { get; private set; } = Guid.NewGuid();

        //When the event was created (useful for logging, debugging, or event ordering)
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        //Lets you trace a request across multiple services (e.g., Order → Payment → Notification).
        public string? CorrelationId { get; set; }
    }
}
