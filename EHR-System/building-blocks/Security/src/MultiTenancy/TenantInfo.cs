using System;
using System.Collections.Generic;

namespace EHRPlatform.Security.MultiTenancy;

/// <summary>
/// Tenant information data structure.
/// Single responsibility: Tenant metadata.
/// </summary>
public class TenantInfo
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Tenant name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Tenant status.
    /// </summary>
    public TenantStatus Status { get; set; }

    /// <summary>
    /// Subscription plan.
    /// </summary>
    public string SubscriptionPlan { get; set; } = null!;

    /// <summary>
    /// Features available to tenant.
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Tenant creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Is tenant active.
    /// </summary>
    public bool IsActive { get; set; }
}
