using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class Report : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ReportType { get; set; }
    public string? Schedule { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    public ICollection<AnalyticsMetric> Metrics { get; set; } = new List<AnalyticsMetric>();
}

