#nullable enable

using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Domain.Events;

/// <summary>
/// Base class for domain events.
/// Domain events represent something that happened in the business domain.
/// Single responsibility: Define domain event contract only.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// When the event occurred in UTC.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// Unique event ID for tracking and deduplication.
    /// </summary>
    public Guid EventId { get; set; } = GuidHelper.NewGuid();

    /// <summary>
    /// Correlation ID for linking related events across services.
    /// </summary>
    public string? CorrelationId { get; set; }
}
