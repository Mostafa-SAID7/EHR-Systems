using System;
using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Health check response for service readiness/liveness probes.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Overall health status.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Service name.
    /// </summary>
    public string Service { get; set; } = null!;

    /// <summary>
    /// Service version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Response timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Individual component health checks.
    /// </summary>
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();

    public HealthCheckResponse()
    {
    }

    public HealthCheckResponse(string status, string service, string? version = null)
    {
        Status = status;
        Service = service;
        Version = version;
    }

    /// <summary>
    /// Add component health check.
    /// </summary>
    public void AddComponent(string name, string status, string? description = null)
    {
        Components[name] = new ComponentHealth
        {
            Status = status,
            Description = description,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Individual component health status.
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
