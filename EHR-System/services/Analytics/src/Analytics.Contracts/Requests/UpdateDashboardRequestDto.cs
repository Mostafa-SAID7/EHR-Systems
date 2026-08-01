namespace EHRPlatform.Services.Analytics.Contracts.Requests;

/// <summary>
/// Request DTO for updating dashboard
/// </summary>
public class UpdateDashboardRequestDto
{
    /// <summary>Gets or sets dashboard name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets dashboard description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets dashboard configuration.</summary>
    public object? Configuration { get; set; }
}
