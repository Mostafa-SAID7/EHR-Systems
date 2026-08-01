namespace EHRPlatform.Observability.Logging;

/// <summary>
/// Log level enumeration.
/// Single responsibility: Log severity values.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Trace level - very detailed diagnostic information.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Debug level - detailed diagnostic information.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Information level - general informational messages.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Warning level - warning messages for potentially problematic situations.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error level - error messages for error events.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical level - critical error messages.
    /// </summary>
    Critical = 5
}
