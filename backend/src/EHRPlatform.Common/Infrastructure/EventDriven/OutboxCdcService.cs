#nullable enable

using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.CDC;

/// <summary>
/// Default ICdcService implementation that fans out to registered sinks.
///
/// Used by microservices to forward committed Outbox events to:
///   - Kafka topics consumed by the Analytics service
///   - Elasticsearch indexers (patient search, clinical notes)
///   - Future Snowflake ETL sinks (added by registering an ICdcSink)
///
/// Sink failures are isolated: one bad sink never blocks the others.
/// </summary>
public sealed class OutboxCdcService : ICdcService
{
    private readonly List<ICdcSink> _sinks = [];
    private readonly ILogger<OutboxCdcService> _logger;

    public OutboxCdcService(ILogger<OutboxCdcService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void RegisterSink(ICdcSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        _sinks.Add(sink);
        _logger.LogInformation("CDC sink registered: {SinkName}", sink.SinkName);
    }

    /// <inheritdoc/>
    public async Task PublishChangeAsync(CdcChangeEvent changeEvent, CancellationToken cancellationToken = default)
    {
        if (_sinks.Count == 0)
        {
            _logger.LogDebug("No CDC sinks registered — skipping change event {EventId}", changeEvent.EventId);
            return;
        }

        _logger.LogDebug(
            "CDC publishing {Operation} on {EntityType} [{EntityId}] to {SinkCount} sink(s)",
            changeEvent.Operation, changeEvent.EntityType, changeEvent.EntityId, _sinks.Count);

        foreach (var sink in _sinks)
        {
            try
            {
                await sink.HandleAsync(changeEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Isolated failure — log and continue to next sink
                _logger.LogError(ex,
                    "CDC sink [{SinkName}] failed for event {EventId} ({EntityType}/{EntityId})",
                    sink.SinkName, changeEvent.EventId, changeEvent.EntityType, changeEvent.EntityId);
            }
        }
    }
}
