#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Result of an audited action.
/// </summary>
public enum AuditResult
{
    /// <summary>
    /// Action completed successfully.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Action was denied due to insufficient permissions.
    /// </summary>
    Denied = 2,

    /// <summary>
    /// Action failed with an error.
    /// </summary>
    Failure = 3,

    /// <summary>
    /// Action was partially successful.
    /// </summary>
    PartialSuccess = 4,

    /// <summary>
    /// Action generated a warning but succeeded.
    /// </summary>
    Warning = 5
}
