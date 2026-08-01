namespace EHRPlatform.Services.Notification.Infrastructure.Kafka;

using MassTransit;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Kafka consumer that listens to domain events and triggers notifications.
/// </summary>
public class NotificationEventConsumer : IConsumer<DomainEventNotification>
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationEventConsumer> _logger;

    public NotificationEventConsumer(IMediator mediator, ILogger<NotificationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DomainEventNotification> context)
    {
        _logger.LogInformation("Consuming notification event: {EventType} for {RecipientId}",
            context.Message.EventType, context.Message.RecipientId);

        try
        {
            // Map domain event to notification command
            var commands = MapEventToNotifications(context.Message);

            foreach (var command in commands)
            {
                var result = await _mediator.Send(command);
                if (!result.Success)
                {
                    _logger.LogWarning("Failed to send notification for event {EventType}", context.Message.EventType);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming notification event");
        }
    }

    private List<SendNotificationCommand> MapEventToNotifications(DomainEventNotification eventNotification)
    {
        var commands = new List<SendNotificationCommand>();

        // Map specific events to notifications
        switch (eventNotification.EventType)
        {
            case "UserCreated":
                commands.Add(new SendNotificationCommand
                {
                    RecipientId = eventNotification.RecipientId,
                    Channel = "Email",
                    NotificationType = "EmailVerification",
                    Subject = "Verify your email",
                    Body = "Please verify your email address to complete registration."
                });
                break;

            case "AppointmentScheduled":
                commands.Add(new SendNotificationCommand
                {
                    RecipientId = eventNotification.RecipientId,
                    Channel = "Email",
                    NotificationType = "AppointmentConfirmation",
                    Subject = "Appointment confirmed",
                    Body = $"Your appointment is scheduled for {eventNotification.Data}"
                });
                commands.Add(new SendNotificationCommand
                {
                    RecipientId = eventNotification.RecipientId,
                    Channel = "SMS",
                    NotificationType = "AppointmentReminder",
                    Body = "Reminder: You have an appointment tomorrow"
                });
                break;

            case "InvoiceCreated":
                commands.Add(new SendNotificationCommand
                {
                    RecipientId = eventNotification.RecipientId,
                    Channel = "Email",
                    NotificationType = "InvoiceNotification",
                    Subject = "New invoice",
                    Body = "Your invoice is ready. Please review and pay if necessary."
                });
                break;

            case "ClinicalResultReady":
                commands.Add(new SendNotificationCommand
                {
                    RecipientId = eventNotification.RecipientId,
                    Channel = "Email",
                    NotificationType = "ResultNotification",
                    Subject = "Your clinical results are ready",
                    Body = "Your clinical results are now available in your patient portal."
                });
                break;
        }

        return commands;
    }
}

/// <summary>
/// Domain event notification message contract.
/// </summary>
public interface DomainEventNotification
{
    Guid EventId { get; }
    string EventType { get; }
    Guid RecipientId { get; }
    string? Data { get; }
    DateTime OccurredAt { get; }
}
