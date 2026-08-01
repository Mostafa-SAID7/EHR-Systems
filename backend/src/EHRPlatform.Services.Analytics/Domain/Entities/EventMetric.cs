using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class EventMetric : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public long Count { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Guid AggregateId { get; set; }
    public Dictionary<string, string>? Properties { get; set; }
}


