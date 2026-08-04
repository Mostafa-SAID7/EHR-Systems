namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// ReportExecution - Report run execution tracking
/// </summary>
public class ReportExecution
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long DurationMs { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed, Running
    public int RecordCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputPath { get; set; } // S3/Blob storage path
    public string? ContentType { get; set; } // application/pdf, text/csv, application/json

    public Report Report { get; set; } = null!;
}
