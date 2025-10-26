using Messaging.Common.Events;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Contracts.Messaging;
using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Application.Messaging
{
    // Handles saga outcome events by preparing structured notification requests
    // and delegating them to the NotificationService for delivery (Email/SMS).
    // This class acts as the application-level coordinator between
    // the messaging infrastructure (RabbitMQ consumers) and domain logic.
    public class NotificationServiceHandler : INotificationServiceHandler
    {
        private readonly INotificationService _notificationService;

        // Injects INotificationService via constructor dependency injection.
        // This allows NotificationServiceHandler to call the CreateAsync() method
        // to persist and dispatch customer notifications.
        public NotificationServiceHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // -----------------------------------------------------------------------
        //    HandleOrderConfirmedAsync
        // -----------------------------------------------------------------------
        // This method is called when the Orchestrator publishes an OrderConfirmedEvent.
        // It constructs a structured notification request with template data and recipient info.
        // The result is saved and queued for delivery by the NotificationService.
        public async Task HandleOrderConfirmedAsync(OrderConfirmedEvent evt)
        {
            // STEP 1: Build a structured list of order items.
            // Each item includes ProductId, Quantity, and Price.
            // (If you have product names available, replace ProductId with Name.)
            var items = evt.Items.Select(i => new
            {
                Name = i.ProductId.ToString(),  // Replace with product name if available
                Quantity = i.Quantity,
                Price = i.UnitPrice
            }).ToList();

            // STEP 2: Serialize the items list into JSON format.
            // This allows the templating engine (e.g., Razor/Handlebars) to easily iterate over them.
            var itemsJson = JsonSerializer.Serialize(items);

            // STEP 3: Prepare the template data dictionary.
            // These key-value pairs will be injected into the notification template placeholders.
            var templateData = new Dictionary<string, object>
            {
                { "CustomerName", evt.CustomerName },
                { "OrderNumber", evt.OrderNumber?.ToString() ?? string.Empty },
                { "Amount", evt.TotalAmount },
                { "Items", JsonDocument.Parse(itemsJson).RootElement } // Embed structured items as JSON
            };

            // STEP 4: Build a CreateNotificationRequestDTO object.
            // This DTO contains all metadata needed to send a notification.
            var request = new CreateNotificationRequestDTO
            {
                UserId = evt.UserId,
                TypeId = (int)NotificationTypeEnum.OrderPlaced,   // Notification Type: Order Placed/Confirmed
                Channel = NotificationChannelEnum.Email,          // Medium: Email (can extend to SMS)
                TemplateVersion = 1,                              // Template versioning for flexibility
                TemplateData = templateData,                      // The dynamic content for the message
                Recipients = new List<RecipientDTO>
                {
                    new RecipientDTO
                    {
                        RecipientTypeId = (int)RecipientTypeEnum.To,
                        Email = evt.CustomerEmail,
                        PhoneNumber = evt.PhoneNumber
                    }
                },
                Priority = NotificationPriorityEnum.Normal,        // Priority flag for delivery queue
                ScheduledAtUtc = null,                             // Can be used for delayed notifications
                CreatedBy = "NotificationServiceHandler"           // Audit info for tracking source
            };

            // STEP 5: Persist and dispatch the notification through the core notification service.
            // This will save the notification and trigger downstream delivery (email/SMS).
            await _notificationService.CreateAsync(request);
        }

        // -----------------------------------------------------------------------
        //    HandleOrderCancelledAsync
        // -----------------------------------------------------------------------
        // This method handles the OrderCancelledEvent published by the Orchestrator.
        // It builds a failure notification with cancellation details (e.g., reason).
        public async Task HandleOrderCancelledAsync(OrderCancelledEvent evt)
        {
            // STEP 1: Prepare template placeholders for the cancellation notification.
            var templateData = new Dictionary<string, object>
            {
                { "CustomerName", evt.CustomerName },
                { "OrderNumber", evt.OrderNumber?.ToString() ?? string.Empty },
                { "Amount", evt.TotalAmount },
                { "Reason", evt.Reason }  // Reason for cancellation (e.g., stock shortage)
            };

            // STEP 2: Create the DTO representing a cancellation notification.
            var request = new CreateNotificationRequestDTO
            {
                UserId = evt.UserId,
                TypeId = (int)NotificationTypeEnum.OrderCancelled, // Notification Type: Order Cancelled
                Channel = NotificationChannelEnum.Email,           // Notification medium
                TemplateVersion = 1,                               // Version of the template
                TemplateData = templateData,                       // Dynamic message data
                Recipients = new List<RecipientDTO>
                {
                    new RecipientDTO
                    {
                        RecipientTypeId = (int)RecipientTypeEnum.To,
                        Email = evt.CustomerEmail,
                        PhoneNumber = evt.PhoneNumber
                    }
                },
                Priority = NotificationPriorityEnum.Normal,
                ScheduledAtUtc = null,
                CreatedBy = "NotificationServiceHandler"
            };

            // STEP 3: Save and queue the cancellation notification for delivery.
            await _notificationService.CreateAsync(request);
        }
    }
}
