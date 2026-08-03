namespace Identity.Domain.Enums;

/// <summary>
/// Enumeration for role types in the system
/// </summary>
public enum RoleType
{
    /// <summary>
    /// System administrator with full access
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Healthcare provider (doctor, nurse, etc.)
    /// </summary>
    Provider = 2,

    /// <summary>
    /// Patient receiving healthcare
    /// </summary>
    Patient = 3,

    /// <summary>
    /// Administrative staff
    /// </summary>
    Staff = 4,

    /// <summary>
    /// Clinic/Hospital management
    /// </summary>
    Manager = 5,

    /// <summary>
    /// Billing and finance staff
    /// </summary>
    Billing = 6,

    /// <summary>
    /// Read-only access role
    /// </summary>
    Viewer = 7
}
