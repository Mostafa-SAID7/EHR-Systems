using EHRPlatform.Common.Application.Features.EventDriven.Outbox;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Cleans up published events based on age.
/// Single responsibility: Age-based cleanup logic only.
/// </summary>
public class AgeBasedCleanupStrategy : IOutboxCleanupStrategy
{
    private readonly int _retentionDays;
    private readonly ILogger<AgeBasedCleanupStrategy> _logger;

    public AgeBasedCleanupStrategy(int retentionDays = 30, ILogger<AgeBasedCleanupStrategy>? logger = null)
    {
        _retentionDays = retentionDays > 0 ? retentionDays : 30;
        _logger = logger!;
    }

    public async Task CleanupAsync(IOutboxRepository outboxRepository, CancellationToken cancellationToken = default)
    {
        try
        {
            await outboxRepository.DeletePublishedOlderThanAsync(_retentionDays, cancellationToken);
            _logger?.LogInformation(
                "Cleaned up published outbox events older than {Days} days",
                _retentionDays);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during outbox cleanup");
        }
    }
}
