namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// Error severity enumeration.
/// Single responsibility: Error severity level values.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Fatal error - system cannot continue.
    /// </summary>
    Fatal = 0,

    /// <summary>
    /// Error - operation failed.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Warning - unexpected but recoverable.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Info - informational message.
    /// </summary>
    Info = 3
}
