using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Contract for outbox message processing.
/// Single responsibility: Outbox processing orchestration interface.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Process unpublished outbox messages.
    /// </summary>
    Task ProcessUnpublishedMessagesAsync(CancellationToken cancellationToken);
}
