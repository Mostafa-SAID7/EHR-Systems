using System;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Individual component health status.
/// Single responsibility: Component health status structure.
/// </summary>
public class ComponentHealth
{
    /// <summary>
    /// Component status (Healthy, Degraded, Unhealthy).
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Description or error message.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp of check.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
