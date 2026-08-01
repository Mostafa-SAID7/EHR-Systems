namespace EHRPlatform.Services.Analytics.Application.Analytics.Responses;

/// <summary>
/// Response DTO for Dashboard.
/// </summary>
public class DashboardResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
