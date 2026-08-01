using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;

namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

/// <summary>
/// Set notification preference handler.
/// Manages user notification channel preferences (Email, SMS, Push, etc).
/// Logs preference changes for audit.
/// </summary>
public class SetNotificationPreferenceCommandHandler : ICommandHandler<SetNotificationPreferenceCommand, NotificationResult>
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

    public async Task<NotificationResult> Handle(
        SetNotificationPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Setting notification preferences for user {UserId}: Channels={Channels}",
                command.UserId, string.Join(",", command.PreferredChannels));

            if (command.UserId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty");

            if (command.PreferredChannels == null || command.PreferredChannels.Count == 0)
                throw new ArgumentException("At least one preferred channel must be selected");

            // Validate channel types
            var validChannels = new[] { "Email", "SMS", "Push", "InApp", "Slack", "Teams" };
            foreach (var channel in command.PreferredChannels)
            {
                if (!validChannels.Contains(channel))
                    throw new ArgumentException($"Invalid notification channel: {channel}");
            }

            var repository = _unitOfWork.Repository<Domain.NotificationPreference>();
            var preference = await repository.FirstOrDefaultAsync(
                q => q.Where(p => p.UserId == command.UserId),
                cancellationToken);

            if (preference == null)
            {
                // Create new preference
                preference = new Domain.NotificationPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = command.UserId,
                    PreferredChannels = command.PreferredChannels,
                    OptInAll = command.OptInAll ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await repository.AddAsync(preference, cancellationToken);
            }
            else
            {
                // Update existing preference
                preference.PreferredChannels = command.PreferredChannels;
                preference.OptInAll = command.OptInAll ?? false;
                preference.UpdatedAt = DateTime.UtcNow;
                await repository.UpdateAsync(preference, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Notification preferences updated for user {UserId}",
                command.UserId);

            return new NotificationResult
            {
                Success = true,
                Message = "Notification preferences updated successfully",
                Data = new { preferenceId = preference.Id, channels = preference.PreferredChannels }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification preferences for user {UserId}", command.UserId);
            return new NotificationResult
            {
                Success = false,
                Message = $"Error updating preferences: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Notification domain model - preference
/// </summary>
namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Domain
{
    public class NotificationPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<string> PreferredChannels { get; set; } = new();
        public bool OptInAll { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
