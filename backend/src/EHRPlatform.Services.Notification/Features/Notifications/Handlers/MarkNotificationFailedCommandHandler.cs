using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;


namespace EHRPlatform.Services.Notification.Features.Notifications.Handlers;

/// <summary>
/// Mark notification failed handler.
/// Single Responsibility: Handle delivery failures with exponential backoff retry.
/// </summary>
public class MarkNotificationFailedCommandHandler : ICommandHandler<MarkNotificationFailedCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<MarkNotificationFailedCommandHandler> _logger;

    public MarkNotificationFailedCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<MarkNotificationFailedCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(MarkNotificationFailedCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking notification {NotificationId} as failed: {Reason}",
            command.NotificationId, command.Reason);

        var repo = _unitOfWork.Repository<NotificationEntity>();
        var notification = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == command.NotificationId),
            cancellationToken);

        if (notification == null)
            throw new InvalidOperationException($"Notification {command.NotificationId} not found");

        notification.MarkFailed(command.Reason);
        await repo.UpdateAsync(notification, cancellationToken);

        // Publish event if finally failed
        if (notification.Status == "Failed")
        {
            var failedEvent = notification.GetDomainEvents().Last();
            await _outbox.AddAsync(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = notification.Id,
                EventType = nameof(NotificationFailedEvent),
                EventData = System.Text.Json.JsonSerializer.Serialize(failedEvent),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


