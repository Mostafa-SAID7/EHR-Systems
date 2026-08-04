namespace EHRPlatform.Services.Analytics.Contracts.Responses;

public class CreateDashboardResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? DashboardId { get; set; }
}
