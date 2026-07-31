#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Access level for audit trail classification and permission boundaries.
/// Used to categorize which audit records can be accessed at each permission level.
/// </summary>
public enum AccessLevel
{
    /// <summary>No access - default/uninitialized state.</summary>
    None = 0,

    /// <summary>Audit-only access - can view audit logs and system events.</summary>
    Audit = 1,

    /// <summary>Clinical access - can view and modify clinical data.</summary>
    Clinical = 2,

    /// <summary>Administrative access - can view administrative records and system settings.</summary>
    Administrative = 3,

    /// <summary>Full access - unrestricted access to all system data.</summary>
    Full = 4
}
