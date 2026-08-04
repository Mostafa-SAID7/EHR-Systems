namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// Report - Scheduled report definitions
/// </summary>
public class Report
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QueryDefinition { get; set; } = string.Empty; // JSON query
    public string ReportType { get; set; } = string.Empty; // Daily, Weekly, Monthly, OnDemand
    public string Status { get; set; } = "Active"; // Active, Inactive, Archived
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReportExecution> Executions { get; } = new List<ReportExecution>();
}
