namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event raised when dashboard is deleted
/// </summary>
public record DashboardDeletedEvent(
    Guid DashboardId,
    string DashboardName,
    Guid DeletedBy,
    long TenantId,
    DateTime DeletedAt);

