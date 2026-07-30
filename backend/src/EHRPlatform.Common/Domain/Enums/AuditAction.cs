#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Types of actions that can be audited.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Create new resource.
    /// </summary>
    Create = 1,

    /// <summary>
    /// Read/view existing resource.
    /// </summary>
    Read = 2,

    /// <summary>
    /// Update existing resource.
    /// </summary>
    Update = 3,

    /// <summary>
    /// Delete resource (soft or hard).
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Export data outside the system.
    /// </summary>
    Export = 5,

    /// <summary>
    /// Download file or report.
    /// </summary>
    Download = 6,

    /// <summary>
    /// Print document or report.
    /// </summary>
    Print = 7,

    /// <summary>
    /// Send or share data.
    /// </summary>
    Share = 8,

    /// <summary>
    /// Access control action (login, logout, permission change).
    /// </summary>
    AccessControl = 9,

    /// <summary>
    /// Configuration change.
    /// </summary>
    Configure = 10,

    /// <summary>
    /// Administrative action.
    /// </summary>
    Admin = 11,

    /// <summary>
    /// Consent-related action.
    /// </summary>
    Consent = 12
}
