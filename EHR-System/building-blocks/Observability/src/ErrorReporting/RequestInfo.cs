using System.Collections.Generic;

namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// Request information for error context.
/// Single responsibility: Request error context data.
/// </summary>
public class RequestInfo
{
    /// <summary>
    /// Request method.
    /// </summary>
    public string Method { get; set; } = null!;

    /// <summary>
    /// Request URL.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Query string.
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// Request headers.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}
