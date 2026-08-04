using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;

namespace EHRPlatform.Services.Analytics.Persistence.Repositories;

/// <summary>
/// Repository implementation for KPI entity
/// </summary>
public class KPIRepository : IKPIRepository
{
    private readonly IAnalyticsDbContext _context;

    public KPIRepository(IAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<KPISummary?> GetByIdAsync(Guid id)
    {
        return await _context.KPISummaries
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<KPISummary>> GetAllAsync(long tenantId)
    {
        return await _context.KPISummaries
            .ToListAsync();
    }

    public async Task<IEnumerable<KPISummary>> GetByCategoryAsync(string category, long tenantId)
    {
        return await _context.KPISummaries
            .ToListAsync();
    }

    public async Task<IEnumerable<KPISummary>> GetByDashboardAsync(Guid dashboardId)
    {
        return await _context.KPISummaries
            .ToListAsync();
    }

    public async Task<KPISummary?> GetByNameAsync(string name, long tenantId)
    {
        return await _context.KPISummaries
            .FirstOrDefaultAsync(k => k.SummaryDate.Date == DateTime.UtcNow.Date);
    }

    public async Task<KPISummary> AddAsync(KPISummary kpi)
    {
        _context.KPISummaries.Add(kpi);
        await _context.SaveChangesAsync();
        return kpi;
    }

    public async Task<KPISummary> UpdateAsync(KPISummary kpi)
    {
        _context.KPISummaries.Update(kpi);
        await _context.SaveChangesAsync();
        return kpi;
    }

    public async Task DeleteAsync(Guid id)
    {
        var kpi = await GetByIdAsync(id);
        if (kpi != null)
        {
            _context.KPISummaries.Remove(kpi);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.KPISummaries.AnyAsync(k => k.Id == id);
    }
}
