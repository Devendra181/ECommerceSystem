namespace NotificationService.Contracts.Interfaces
{
    // Defines contract for processing queued notifications.
    public interface INotificationProcessor
    {
        // Process a batch of pending notifications from the queue.
        // take: Max number of notifications to process
        // skip: Number of notifications to skip (for pagination)
        Task ProcessQueueBatchAsync(int take, int skip);
    }
}
