using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;

namespace EHRPlatform.Services.Analytics.Persistence.Repositories;

/// <summary>
/// Repository implementation for Dashboard entity
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly IAnalyticsDbContext _context;

    public DashboardRepository(IAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<Dashboard?> GetByIdAsync(Guid id)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Dashboard>> GetAllAsync(long tenantId)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dashboard>> GetActiveAsync(long tenantId)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .Where(d => d.IsPublic || d.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
    }

    public async Task<Dashboard?> GetByNameAsync(string name, long tenantId)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Name == name);
    }

    public async Task<Dashboard> AddAsync(Dashboard dashboard)
    {
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();
        return dashboard;
    }

    public async Task<Dashboard> UpdateAsync(Dashboard dashboard)
    {
        _context.Dashboards.Update(dashboard);
        await _context.SaveChangesAsync();
        return dashboard;
    }

    public async Task DeleteAsync(Guid id)
    {
        var dashboard = await GetByIdAsync(id);
        if (dashboard != null)
        {
            _context.Dashboards.Remove(dashboard);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Dashboards.AnyAsync(d => d.Id == id);
    }
}
