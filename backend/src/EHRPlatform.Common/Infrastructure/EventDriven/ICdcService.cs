#nullable enable

namespace EHRPlatform.Common.CDC;

/// <summary>
/// Change Data Capture (CDC) abstraction.
///
/// In this platform CDC is implemented via the Outbox pattern:
/// every committed domain change produces an <see cref="Events.OutboxEvent"/> row.
/// The <see cref="OutboxCdcService"/> reads those rows and forwards them to
/// downstream sinks (Kafka topics, Snowflake staging tables, Elasticsearch).
///
/// Implementations can be swapped for a Debezium connector without changing callers.
/// </summary>
public interface ICdcService
{
    /// <summary>
    /// Publish a change event to all registered downstream sinks.
    /// Called by the OutboxCdcService after reading a committed OutboxEvent.
    /// </summary>
    /// <param name="changeEvent">The captured change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishChangeAsync(CdcChangeEvent changeEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a downstream sink that receives every change event.
    /// Sinks are called sequentially; failures are isolated per sink.
    /// </summary>
    void RegisterSink(ICdcSink sink);
}

/// <summary>
/// A downstream consumer of CDC change events (e.g. Snowflake ETL, Elasticsearch indexer).
/// </summary>
public interface ICdcSink
{
    string SinkName { get; }

    Task HandleAsync(CdcChangeEvent change, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single captured row change.
/// </summary>
public sealed class CdcChangeEvent
{
    /// <summary>Unique event identifier (mirrors OutboxEvent.Id).</summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>Fully-qualified entity type name (e.g. "Patient", "Appointment").</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Primary key of the changed entity.</summary>
    public string EntityId { get; init; } = string.Empty;

    /// <summary>CREATE | UPDATE | DELETE | SOFT_DELETE</summary>
    public CdcOperation Operation { get; init; }

    /// <summary>Serialized entity snapshot AFTER the change (null for DELETE).</summary>
    public string? PayloadJson { get; init; }

    /// <summary>Source microservice (e.g. "patient-service").</summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>Tenant / organisation context.</summary>
    public Guid? TenantId { get; init; }

    /// <summary>User who triggered the change.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>UTC timestamp of the change.</summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>Correlation ID for distributed tracing.</summary>
    public string? CorrelationId { get; init; }
}

public enum CdcOperation
{
    Create,
    Update,
    Delete,
    SoftDelete
}
