using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;


namespace EHRPlatform.Services.Notification.Features.Notifications.Handlers;

/// <summary>
/// Mark notification sent handler.
/// Single Responsibility: Update delivery status and emit integration event.
/// </summary>
public class MarkNotificationSentCommandHandler : ICommandHandler<MarkNotificationSentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<MarkNotificationSentCommandHandler> _logger;

    public MarkNotificationSentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<MarkNotificationSentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(MarkNotificationSentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking notification {NotificationId} as sent", command.NotificationId);

        var repo = _unitOfWork.Repository<Notification>();
        var notification = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == command.NotificationId),
            cancellationToken);

        if (notification == null)
            throw new InvalidOperationException($"Notification {command.NotificationId} not found");

        notification.MarkSent(command.MessageId ?? "");
        await repo.UpdateAsync(notification, cancellationToken);

        // Publish event
        var sentEvent = notification.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = notification.Id,
            EventType = nameof(NotificationSentEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(sentEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
