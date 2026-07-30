using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class AnalyticsMetric : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Value { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // For aggregation
    public string? MetricName { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? Unit { get; set; }
    public string? Frequency { get; set; }
}

