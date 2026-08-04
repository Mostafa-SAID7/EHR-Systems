namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.BuildingBlocks.Caching;

/// <summary>
/// Handler for GetDashboardByIdQuery
/// </summary>
public class GetDashboardByIdQueryHandler : IRequestHandler<GetDashboardByIdQuery, GetDashboardByIdResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetDashboardByIdQueryHandler> _logger;

    public GetDashboardByIdQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        ILogger<GetDashboardByIdQueryHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GetDashboardByIdResponse> Handle(GetDashboardByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dashboard {DashboardId}", request.DashboardId);

        try
        {
            // Check cache
            var cacheKey = $"dashboard:{request.DashboardId}";
            var cachedDashboard = await _cacheService.GetAsync<DashboardDetailDto>(cacheKey);

            if (cachedDashboard != null)
            {
                _logger.LogInformation("Retrieved dashboard from cache");
                return new GetDashboardByIdResponse
                {
                    Success = true,
                    Dashboard = cachedDashboard
                };
            }

            // Get from repository
            var dashboard = await _dashboardRepository.GetByIdAsync(request.DashboardId);

            if (dashboard == null)
            {
                return new GetDashboardByIdResponse
                {
                    Success = false,
                    Message = $"Dashboard {request.DashboardId} not found"
                };
            }

            var dto = new DashboardDetailDto
            {
                Id = dashboard.Id,
                Name = dashboard.Name,
                Description = dashboard.Description,
                IsPublic = dashboard.IsPublic,
                CreatedAt = dashboard.CreatedAt,
                UpdatedAt = dashboard.UpdatedAt,
                CreatedBy = dashboard.CreatedBy,
                Widgets = dashboard.Widgets?.Select(w => new WidgetDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    WidgetType = w.WidgetType,
                    Position = w.Position,
                    SizeWidth = w.SizeWidth,
                    SizeHeight = w.SizeHeight
                }).ToList() ?? new()
            };

            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

            return new GetDashboardByIdResponse
            {
                Success = true,
                Dashboard = dto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard {DashboardId}", request.DashboardId);
            return new GetDashboardByIdResponse
            {
                Success = false,
                Message = $"Failed to get dashboard: {ex.Message}"
            };
        }
    }
}
