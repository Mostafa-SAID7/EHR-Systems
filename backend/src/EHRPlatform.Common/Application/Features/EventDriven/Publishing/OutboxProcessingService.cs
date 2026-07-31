using EHRPlatform.Common.Application.Features.EventDriven.Outbox;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Orchestrates outbox event processing (fetch, publish, cleanup).
/// Single responsibility: Processing orchestration only.
/// </summary>
public class OutboxProcessingService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxEventPublisher _eventPublisher;
    private readonly IOutboxCleanupStrategy _cleanupStrategy;
    private readonly ILogger<OutboxProcessingService> _logger;

    public OutboxProcessingService(
        IOutboxRepository outboxRepository,
        IOutboxEventPublisher eventPublisher,
        IOutboxCleanupStrategy cleanupStrategy,
        ILogger<OutboxProcessingService> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _cleanupStrategy = cleanupStrategy ?? throw new ArgumentNullException(nameof(cleanupStrategy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Process one batch of pending outbox events.
    /// 1. Fetch unpublished events
    /// 2. Publish each event
    /// 3. Clean up published events
    /// 4. Report status
    /// </summary>
    public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch pending events
            var pending = (await _outboxRepository.GetUnpublishedAsync(cancellationToken)).ToList();

            if (pending.Count > 0)
            {
                _logger.LogInformation("Processing {Count} outbox events", pending.Count);

                // Publish each event
                await _eventPublisher.PublishBatchAsync(pending, cancellationToken);
            }

            // Cleanup old published events
            await _cleanupStrategy.CleanupAsync(_outboxRepository, cancellationToken);

            // Report status
            var stillPending = await _outboxRepository.GetPendingCountAsync(cancellationToken);
            if (stillPending > 0)
            {
                _logger.LogInformation("Outbox has {Count} pending events after processing", stillPending);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox batch");
        }
    }
}
