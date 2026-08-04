using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for Report entity
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Get report by ID
    /// </summary>
    Task<Report?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all reports for tenant
    /// </summary>
    Task<IEnumerable<Report>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get active reports for tenant
    /// </summary>
    Task<IEnumerable<Report>> GetActiveAsync(long tenantId);

    /// <summary>
    /// Get scheduled reports for tenant
    /// </summary>
    Task<IEnumerable<Report>> GetScheduledAsync(long tenantId);

    /// <summary>
    /// Get report by name
    /// </summary>
    Task<Report?> GetByNameAsync(string name, long tenantId);

    /// <summary>
    /// Get reports by creator
    /// </summary>
    Task<IEnumerable<Report>> GetByCreatorAsync(Guid createdBy, long tenantId);

    /// <summary>
    /// Get reports by type
    /// </summary>
    Task<IEnumerable<Report>> GetByTypeAsync(string reportType, long tenantId);

    /// <summary>
    /// Add new report
    /// </summary>
    Task<Report> AddAsync(Report report);

    /// <summary>
    /// Update existing report
    /// </summary>
    Task<Report> UpdateAsync(Report report);

    /// <summary>
    /// Delete report
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if report exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Get report with all executions
    /// </summary>
    Task<Report?> GetWithExecutionsAsync(Guid id);

    /// <summary>
    /// Get reports created within date range
    /// </summary>
    Task<IEnumerable<Report>> GetCreatedBetweenAsync(DateTime startDate, DateTime endDate, long tenantId);

    /// <summary>
    /// Get reports updated recently (last N days)
    /// </summary>
    Task<IEnumerable<Report>> GetUpdatedRecentlyAsync(int lastDays, long tenantId);

    /// <summary>
    /// Archive reports older than specified days
    /// </summary>
    Task ArchiveOldReportsAsync(int olderThanDays, long tenantId);
}
