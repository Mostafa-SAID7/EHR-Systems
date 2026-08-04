namespace EHRPlatform.Services.Analytics.Contracts.Responses;

public class GetMetricsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IEnumerable<MetricDataDto> Metrics { get; set; } = new List<MetricDataDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class MetricDataDto
{
    public Guid Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Dimension1 { get; set; }
    public string? Dimension2 { get; set; }
    public string? Dimension3 { get; set; }
}
