using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class ReportExecution : BaseEntity
{
    public Guid ReportId { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }
    public string Status { get; set; } = "completed"; // completed, failed, running
    public string? ErrorMessage { get; set; }
    public long RecordCount { get; set; }
    
    // Navigation
    public Report? Report { get; set; }
}

