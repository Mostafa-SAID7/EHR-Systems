namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event raised when dashboard is created
/// </summary>
public record DashboardCreatedEvent(
    Guid DashboardId,
    string Name,
    Guid CreatedBy,
    long TenantId,
    DateTime CreatedAt);

