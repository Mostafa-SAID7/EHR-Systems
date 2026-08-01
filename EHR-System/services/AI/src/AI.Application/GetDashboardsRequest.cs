namespace EHRPlatform.Services.Analytics.Application.Analytics.Requests;

/// <summary>
/// Request DTO for getting dashboards.
/// </summary>
public class GetDashboardsRequest
{
    public Guid? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
