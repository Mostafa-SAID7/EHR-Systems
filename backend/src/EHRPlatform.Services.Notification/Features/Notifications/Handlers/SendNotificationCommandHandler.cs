using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;

using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;
using Mapster;

namespace EHRPlatform.Services.Notification.Features.Notifications.Handlers;

/// <summary>
/// Send notification handler.
/// Routes to appropriate channel provider (email, SMS, push, in-app).
/// Single Responsibility: Create notification with user preference validation.
/// </summary>
public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, NotificationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    public SendNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<NotificationResponseDto> Handle(
        SendNotificationCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending notification to {RecipientId} via {Channel}",
            command.RecipientId, command.Channel);

        // Check user preferences
        var prefRepo = _unitOfWork.Repository<NotificationPreference>();
        var preference = await prefRepo.FirstOrDefaultAsync(
            q => q.Where(p =>
                p.UserId == command.RecipientId &&
                p.Channel == command.Channel &&
                p.NotificationType == command.NotificationType),
            cancellationToken);

        if (preference?.IsEnabled == false)
            throw new InvalidOperationException("User has disabled this notification type");

        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            RecipientId = command.RecipientId,
            Channel = command.Channel,
            NotificationType = command.NotificationType,
            Subject = command.Subject,
            Body = command.Body,
            Recipient = command.Recipient,
            ScheduledFor = command.ScheduledFor ?? DateTime.UtcNow,
            TemplateVars = command.TemplateVars ?? new()
        };

        var repo = _unitOfWork.Repository<NotificationEntity>();
        await repo.AddAsync(notification, cancellationToken);

        // Publish event
        var createdEvent = new NotificationCreatedEvent(
            notification.Id, notification.RecipientId, notification.Channel, notification.NotificationType);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = notification.Id,
            EventType = nameof(NotificationCreatedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification created {NotificationId}", notification.Id);

        return notification.Adapt<NotificationResponseDto>();
    }
}

