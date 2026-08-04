namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event fired when a metric is recorded
/// </summary>
public record MetricRecordedEvent(string MetricName, decimal Value, DateTime Timestamp)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
