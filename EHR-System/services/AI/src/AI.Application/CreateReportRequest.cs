namespace EHRPlatform.Services.Analytics.Application.Analytics.Requests;

/// <summary>
/// Request DTO for creating a report.
/// </summary>
public class CreateReportRequest
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? ReportType { get; set; }
    public string? Schedule { get; set; }
    public string? Description { get; set; }
}
