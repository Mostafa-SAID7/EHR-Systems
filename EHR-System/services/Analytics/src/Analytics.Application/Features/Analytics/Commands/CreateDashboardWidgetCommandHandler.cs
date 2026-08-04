namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for creating dashboard widget
/// </summary>
public class CreateDashboardWidgetCommandHandler : IRequestHandler<CreateDashboardWidgetCommand, CreateWidgetResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateDashboardWidgetCommandHandler> _logger;

    public CreateDashboardWidgetCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        ITenantContext tenantContext,
        ILogger<CreateDashboardWidgetCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<CreateWidgetResponse> Handle(
        CreateDashboardWidgetCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating widget for dashboard {DashboardId}", command.DashboardId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Get dashboard
            var dashboard = await _dashboardRepository.GetByIdAsync(command.DashboardId);
            if (dashboard == null)
            {
                throw new InvalidDashboardException($"Dashboard {command.DashboardId} not found");
            }

            // Create widget
            var widget = new DashboardWidget
            {
                Id = Guid.NewGuid(),
                DashboardId = command.DashboardId,
                Title = command.Title,
                WidgetType = command.WidgetType,
                Position = command.Position,
                SizeWidth = command.SizeWidth,
                SizeHeight = command.SizeHeight,
                Configuration = command.Configuration,
                CreatedAt = DateTime.UtcNow
            };

            dashboard.Widgets ??= new();
            dashboard.Widgets.Add(widget);

            await _dashboardRepository.UpdateAsync(dashboard);

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");

            _logger.LogInformation("Widget created successfully: {WidgetId}", widget.Id);

            return new CreateWidgetResponse(
                Success: true,
                Message: "Widget created successfully",
                WidgetId: widget.Id);
        }
        catch (InvalidDashboardException ex)
        {
            _logger.LogWarning(ex, "Dashboard not found: {DashboardId}", command.DashboardId);
            return new CreateWidgetResponse(
                Success: false,
                Message: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating widget for dashboard {DashboardId}", command.DashboardId);
            return new CreateWidgetResponse(
                Success: false,
                Message: $"Failed to create widget: {ex.Message}");
        }
    }
}
