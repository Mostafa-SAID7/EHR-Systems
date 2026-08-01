using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.MultiTenancy;

/// <summary>
/// Interface for managing tenant context in current request/scope.
/// Single responsibility: Store and retrieve tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Get current tenant ID.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Get current tenant info.
    /// </summary>
    TenantInfo? TenantInfo { get; }

    /// <summary>
    /// Set tenant context for current scope.
    /// </summary>
    Task SetTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear tenant context.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if tenant context is set.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Execute action within tenant context.
    /// </summary>
    Task ExecuteAsync(string tenantId, Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute action with result within tenant context.
    /// </summary>
    Task<T> ExecuteAsync<T>(string tenantId, Func<Task<T>> action, CancellationToken cancellationToken = default);
}
