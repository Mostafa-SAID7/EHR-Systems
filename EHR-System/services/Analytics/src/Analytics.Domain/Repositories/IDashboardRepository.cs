using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for Dashboard entity
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    Task<Dashboard?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all dashboards for tenant
    /// </summary>
    Task<IEnumerable<Dashboard>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get active dashboards for tenant
    /// </summary>
    Task<IEnumerable<Dashboard>> GetActiveAsync(long tenantId);

    /// <summary>
    /// Get dashboard by name
    /// </summary>
    Task<Dashboard?> GetByNameAsync(string name, long tenantId);

    /// <summary>
    /// Add new dashboard
    /// </summary>
    Task<Dashboard> AddAsync(Dashboard dashboard);

    /// <summary>
    /// Update existing dashboard
    /// </summary>
    Task<Dashboard> UpdateAsync(Dashboard dashboard);

    /// <summary>
    /// Delete dashboard
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if dashboard exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
