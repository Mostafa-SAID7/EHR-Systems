namespace EHRPlatform.Security.MultiTenancy;

/// <summary>
/// Tenant status enumeration.
/// Single responsibility: Tenant status values.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Tenant is suspended.
    /// </summary>
    Suspended = 1,

    /// <summary>
    /// Tenant is inactive.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Tenant is pending activation.
    /// </summary>
    Pending = 3
}
