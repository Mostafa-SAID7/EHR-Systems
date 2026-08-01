namespace EHRPlatform.Services.Analytics.Application.Analytics.Responses;

/// <summary>
/// Response DTO for Report.
/// </summary>
public class ReportResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? ReportType { get; set; }
    public string? Schedule { get; set; }
    public DateTime CreatedAt { get; set; }
}
