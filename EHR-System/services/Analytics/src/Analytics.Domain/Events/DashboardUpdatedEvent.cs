namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event raised when dashboard is updated
/// </summary>
public record DashboardUpdatedEvent(
    Guid DashboardId,
    string? UpdatedName,
    string? UpdatedDescription,
    bool? UpdatedIsPublic,
    Guid UpdatedBy,
    long TenantId,
    DateTime UpdatedAt);

