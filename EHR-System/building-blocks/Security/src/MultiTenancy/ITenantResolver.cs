using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.MultiTenancy;

/// <summary>
/// Interface for resolving current tenant from context.
/// Single responsibility: Determine current tenant ID/context.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Get current tenant ID from context (HTTP, headers, claims, etc).
    /// </summary>
    Task<string?> GetCurrentTenantIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current tenant info with validation.
    /// </summary>
    Task<TenantInfo?> GetCurrentTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate tenant ID is valid/active.
    /// </summary>
    Task<bool> ValidateTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if tenant has feature enabled.
    /// </summary>
    Task<bool> HasFeatureAsync(string tenantId, string featureName, CancellationToken cancellationToken = default);
}
