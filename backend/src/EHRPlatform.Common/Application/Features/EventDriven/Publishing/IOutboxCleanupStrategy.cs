using EHRPlatform.Common.Application.Features.EventDriven.Outbox;

namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Abstraction for outbox cleanup strategies.
/// Single responsibility: Define cleanup contract.
/// </summary>
public interface IOutboxCleanupStrategy
{
    /// <summary>
    /// Clean up published events according to strategy.
    /// </summary>
    Task CleanupAsync(IOutboxRepository outboxRepository, CancellationToken cancellationToken = default);
}
