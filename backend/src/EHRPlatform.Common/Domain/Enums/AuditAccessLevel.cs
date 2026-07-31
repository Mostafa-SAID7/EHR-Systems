namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Access levels for audit trail visibility.
/// Single responsibility: Define audit access level contract only.
/// </summary>
public enum AuditAccessLevel
{
    /// <summary>
    /// Only the user who made the change can access the audit trail.
    /// </summary>
    Personal = 0,

    /// <summary>
    /// Department/team level access.
    /// </summary>
    Department = 1,

    /// <summary>
    /// Standard organizational access.
    /// </summary>
    Standard = 2,

    /// <summary>
    /// Senior management access.
    /// </summary>
    Management = 3,

    /// <summary>
    /// Compliance officer / Auditor access.
    /// </summary>
    Auditor = 4,

    /// <summary>
    /// System administrator access.
    /// </summary>
    Administrator = 5
}
