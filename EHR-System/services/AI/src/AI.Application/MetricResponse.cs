namespace EHRPlatform.Services.Analytics.Application.Analytics.Responses;

/// <summary>
/// Response DTO for Metric.
/// </summary>
public class MetricResponse
{
    public Guid Id { get; set; }
    public string? MetricName { get; set; }
    public string? Category { get; set; }
    public decimal Value { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
