namespace EHRPlatform.Services.Analytics.Contracts.Requests;

public class CreateDashboardRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
}
