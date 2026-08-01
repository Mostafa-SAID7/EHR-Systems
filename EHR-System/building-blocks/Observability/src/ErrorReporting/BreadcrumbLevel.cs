namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// Breadcrumb level enumeration.
/// Single responsibility: Breadcrumb severity level values.
/// </summary>
public enum BreadcrumbLevel
{
    /// <summary>
    /// Debug level.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Info level.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning level.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error level.
    /// </summary>
    Error = 3
}
