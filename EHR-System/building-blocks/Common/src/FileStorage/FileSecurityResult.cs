namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// File security scan result.
/// Single responsibility: Security scan data.
/// </summary>
public class FileSecurityResult
{
    /// <summary>
    /// Is file safe.
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// Threat level (if found).
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>
    /// Details of threats (if any).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Scan engine name.
    /// </summary>
    public string? ScanEngine { get; set; }
}
