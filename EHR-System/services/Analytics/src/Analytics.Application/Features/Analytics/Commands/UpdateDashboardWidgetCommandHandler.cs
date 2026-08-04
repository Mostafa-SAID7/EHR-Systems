namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.BuildingBlocks.Caching;

/// <summary>
/// Handler for updating dashboard widget
/// </summary>
public class UpdateDashboardWidgetCommandHandler : IRequestHandler<UpdateDashboardWidgetCommand, UpdateWidgetResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateDashboardWidgetCommandHandler> _logger;

    public UpdateDashboardWidgetCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        ILogger<UpdateDashboardWidgetCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UpdateWidgetResponse> Handle(
        UpdateDashboardWidgetCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating widget {WidgetId} on dashboard {DashboardId}", 
            command.WidgetId, command.DashboardId);

        try
        {
            // Get dashboard
            var dashboard = await _dashboardRepository.GetByIdAsync(command.DashboardId);
            if (dashboard == null)
            {
                throw new InvalidDashboardException($"Dashboard {command.DashboardId} not found");
            }

            // Find widget
            var widget = dashboard.Widgets?.FirstOrDefault(w => w.Id == command.WidgetId);
            if (widget == null)
            {
                return new UpdateWidgetResponse(
                    Success: false,
                    Message: "Widget not found");
            }

            // Update properties if provided
            if (!string.IsNullOrWhiteSpace(command.Title))
                widget.Title = command.Title;

            if (command.Position >= 0)
                widget.Position = command.Position;

            if (command.SizeWidth > 0)
                widget.SizeWidth = command.SizeWidth;

            if (command.SizeHeight > 0)
                widget.SizeHeight = command.SizeHeight;

            if (!string.IsNullOrWhiteSpace(command.Configuration))
                widget.Configuration = command.Configuration;

            widget.UpdatedAt = DateTime.UtcNow;

            await _dashboardRepository.UpdateAsync(dashboard);

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");

            _logger.LogInformation("Widget updated successfully: {WidgetId}", command.WidgetId);

            return new UpdateWidgetResponse(
                Success: true,
                Message: "Widget updated successfully");
        }
        catch (InvalidDashboardException ex)
        {
            _logger.LogWarning(ex, "Dashboard not found: {DashboardId}", command.DashboardId);
            return new UpdateWidgetResponse(
                Success: false,
                Message: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {WidgetId}", command.WidgetId);
            return new UpdateWidgetResponse(
                Success: false,
                Message: $"Failed to update widget: {ex.Message}");
        }
    }
}
