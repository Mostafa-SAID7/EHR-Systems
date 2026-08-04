using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for KPI entity
/// </summary>
public interface IKPIRepository
{
    /// <summary>
    /// Get KPI by ID
    /// </summary>
    Task<KPI?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all KPIs for tenant
    /// </summary>
    Task<IEnumerable<KPI>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get KPIs by category
    /// </summary>
    Task<IEnumerable<KPI>> GetByCategoryAsync(string category, long tenantId);

    /// <summary>
    /// Get KPIs for dashboard
    /// </summary>
    Task<IEnumerable<KPI>> GetByDashboardAsync(Guid dashboardId);

    /// <summary>
    /// Get KPI by name
    /// </summary>
    Task<KPI?> GetByNameAsync(string name, long tenantId);

    /// <summary>
    /// Add new KPI
    /// </summary>
    Task<KPI> AddAsync(KPI kpi);

    /// <summary>
    /// Update existing KPI
    /// </summary>
    Task<KPI> UpdateAsync(KPI kpi);

    /// <summary>
    /// Delete KPI
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if KPI exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
