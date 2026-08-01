using System.Collections.Generic;

namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// Error context information.
/// Single responsibility: Error context data structure.
/// </summary>
public class ErrorContext
{
    /// <summary>
    /// User information.
    /// </summary>
    public UserInfo? User { get; set; }

    /// <summary>
    /// Request information.
    /// </summary>
    public RequestInfo? Request { get; set; }

    /// <summary>
    /// Additional tags.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Additional data.
    /// </summary>
    public Dictionary<string, object>? Extra { get; set; }
}
