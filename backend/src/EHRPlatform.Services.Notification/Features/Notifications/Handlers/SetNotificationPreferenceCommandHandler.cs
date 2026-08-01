using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;


namespace EHRPlatform.Services.Notification.Features.Notifications.Handlers;

/// <summary>
/// Set notification preference handler.
/// Single Responsibility: Update user opt-in/out preferences.
/// </summary>
public class SetNotificationPreferenceCommandHandler : ICommandHandler<SetNotificationPreferenceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetNotificationPreferenceCommandHandler> _logger;

    public SetNotificationPreferenceCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SetNotificationPreferenceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(SetNotificationPreferenceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting preference for user {UserId}: {Channel}/{Type} = {Enabled}",
            command.UserId, command.Channel, command.NotificationType, command.IsEnabled);

        var prefRepo = _unitOfWork.Repository<NotificationPreference>();
        var preference = await prefRepo.FirstOrDefaultAsync(
            q => q.Where(p =>
                p.UserId == command.UserId &&
                p.Channel == command.Channel &&
                p.NotificationType == command.NotificationType),
            cancellationToken);

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                Channel = command.Channel,
                NotificationType = command.NotificationType,
                IsEnabled = command.IsEnabled
            };
            await prefRepo.AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.IsEnabled = command.IsEnabled;
            await prefRepo.UpdateAsync(preference, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


