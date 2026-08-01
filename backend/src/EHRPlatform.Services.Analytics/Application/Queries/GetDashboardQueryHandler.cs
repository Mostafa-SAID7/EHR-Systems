using EHRPlatform.BuildingBlocks.EventBus.CQRS;
using EHRPlatform.Services.Analytics.Application.Services;
using EHRPlatform.Services.Analytics.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Analytics.Application.Queries;

/// <summary>
/// Query to retrieve a dashboard by ID.
/// Demonstrates caching integration using GetOrSetAsync pattern (prevents thundering herd).
/// </summary>
public record GetDashboardQuery(Guid DashboardId) : IRequest<DashboardDto>;

/// <summary>
/// Handler for GetDashboardQuery.
/// Uses ICacheService (from Common) via IAnalyticsCacheService wrapper.
/// </summary>
public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly AnalyticsContext _context;
    private readonly IAnalyticsCacheService _cacheService;
    private readonly ILogger<GetDashboardQueryHandler> _logger;

    public GetDashboardQueryHandler(
        AnalyticsContext context,
        IAnalyticsCacheService cacheService,
        ILogger<GetDashboardQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    /// <summary>
    /// Handle dashboard query with caching.
    /// If cache miss, loads from database using GetOrSetAsync (atomic, prevents thundering herd).
    /// </summary>
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dashboard: {DashboardId}", request.DashboardId);

        // Use GetOrSetAsync to prevent thundering herd:
        // - If cached: returns instantly
        // - If not cached: calls loader, caches result, returns
        var dashboard = await _cacheService.GetDashboardAsync(
            request.DashboardId,
            loader: async () =>
            {
                _logger.LogInformation("Loading dashboard from database: {DashboardId}", request.DashboardId);
                
                var entity = await _context.Dashboards
                    .Include(d => d.DashboardWidgets)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == request.DashboardId, cancellationToken);

                if (entity == null)
                {
                    _logger.LogWarning("Dashboard not found: {DashboardId}", request.DashboardId);
                    throw new KeyNotFoundException($"Dashboard {request.DashboardId} not found");
                }

                return MapToDto(entity);
            });

        return dashboard ?? throw new InvalidOperationException("Dashboard could not be loaded");
    }

    private static DashboardDto MapToDto(Domain.Entities.Dashboard entity)
    {
        return new DashboardDto(
            entity.Id,
            entity.Name,
            entity.Description ?? string.Empty,
            entity.DashboardWidgets.Select(w => new DashboardWidgetDto(
                w.Id,
                w.Title,
                w.WidgetType,
                w.Config ?? string.Empty
            )).ToList()
        );
    }
}

/// <summary>
/// DTO for dashboard response.
/// </summary>
public record DashboardDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<DashboardWidgetDto> Widgets);

/// <summary>
/// DTO for dashboard widget.
/// </summary>
public record DashboardWidgetDto(
    Guid Id,
    string Title,
    string WidgetType,
    string Config);


