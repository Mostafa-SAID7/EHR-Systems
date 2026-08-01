using System;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Result of a health check.
/// Single responsibility: Health check result data structure.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Is the component healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Component status message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Error details if unhealthy.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Timestamp of check.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
