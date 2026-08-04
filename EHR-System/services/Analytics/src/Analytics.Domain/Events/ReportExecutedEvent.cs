namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event fired when a report is executed
/// </summary>
public record ReportExecutedEvent(Guid ReportId, string Status, int RecordCount)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
