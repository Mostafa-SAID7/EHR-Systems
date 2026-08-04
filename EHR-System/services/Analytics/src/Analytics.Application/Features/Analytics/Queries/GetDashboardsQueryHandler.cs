namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for GetDashboardsQuery
/// </summary>
public class GetDashboardsQueryHandler : IRequestHandler<GetDashboardsQuery, GetDashboardsResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetDashboardsQueryHandler> _logger;

    public GetDashboardsQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        ITenantContext tenantContext,
        ILogger<GetDashboardsQueryHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<GetDashboardsResponse> Handle(GetDashboardsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dashboards - Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new GetDashboardsResponse
                {
                    Success = false,
                    Message = "Tenant context not available"
                };
            }

            // Check cache
            var cacheKey = $"dashboards:all:{tenantId}";
            var cachedDashboards = await _cacheService.GetAsync<List<DashboardListItemDto>>(cacheKey);

            if (cachedDashboards != null && cachedDashboards.Any())
            {
                _logger.LogInformation("Retrieved dashboards from cache");
                return new GetDashboardsResponse
                {
                    Success = true,
                    Dashboards = cachedDashboards,
                    TotalCount = cachedDashboards.Count,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            // Get from repository
            var dashboards = await _dashboardRepository.GetAllAsync(tenantId);

            var dtos = dashboards
                .OrderByDescending(d => d.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new DashboardListItemDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsPublic = d.IsPublic,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    CreatedBy = d.CreatedBy,
                    WidgetCount = d.Widgets?.Count ?? 0
                })
                .ToList();

            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

            return new GetDashboardsResponse
            {
                Success = true,
                Dashboards = dtos,
                TotalCount = dashboards.Count(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboards");
            return new GetDashboardsResponse
            {
                Success = false,
                Message = $"Failed to get dashboards: {ex.Message}"
            };
        }
    }
}
