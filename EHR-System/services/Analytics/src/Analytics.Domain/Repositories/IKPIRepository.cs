using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for KPISummary entity
/// </summary>
public interface IKPIRepository
{
    /// <summary>
    /// Get KPI by ID
    /// </summary>
    Task<KPISummary?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all KPIs for tenant
    /// </summary>
    Task<IEnumerable<KPISummary>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get KPIs by category
    /// </summary>
    Task<IEnumerable<KPISummary>> GetByCategoryAsync(string category, long tenantId);

    /// <summary>
    /// Get KPIs for dashboard
    /// </summary>
    Task<IEnumerable<KPISummary>> GetByDashboardAsync(Guid dashboardId);

    /// <summary>
    /// Get KPI by name
    /// </summary>
    Task<KPISummary?> GetByNameAsync(string name, long tenantId);

    /// <summary>
    /// Add new KPI
    /// </summary>
    Task<KPISummary> AddAsync(KPISummary kpi);

    /// <summary>
    /// Update existing KPI
    /// </summary>
    Task<KPISummary> UpdateAsync(KPISummary kpi);

    /// <summary>
    /// Delete KPI
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if KPI exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
