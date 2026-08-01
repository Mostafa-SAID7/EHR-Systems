using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Overall system health snapshot.
/// Single responsibility: System health aggregation data structure.
/// </summary>
public class SystemHealth
{
    /// <summary>
    /// Overall system status.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// System checks results by component.
    /// </summary>
    public Dictionary<string, HealthCheckResult> Components { get; set; } = new();

    /// <summary>
    /// Number of healthy components.
    /// </summary>
    public int HealthyCount => Components.Values.Count(x => x.IsHealthy);

    /// <summary>
    /// Total number of components.
    /// </summary>
    public int TotalCount => Components.Count;

    /// <summary>
    /// Whether all components are healthy.
    /// </summary>
    public bool IsHealthy => HealthyCount == TotalCount;
}
