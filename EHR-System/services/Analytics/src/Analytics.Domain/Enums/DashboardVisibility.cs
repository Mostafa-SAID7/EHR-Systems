namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for dashboard visibility levels
/// </summary>
public enum DashboardVisibility
{
    /// <summary>Dashboard visible only to creator</summary>
    Private = 0,
    
    /// <summary>Dashboard visible to team/department</summary>
    Team = 1,
    
    /// <summary>Dashboard visible to all organization members</summary>
    Organization = 2,
    
    /// <summary>Dashboard visible to system administrators only</summary>
    AdminOnly = 3
}
