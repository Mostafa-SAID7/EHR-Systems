namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.BuildingBlocks.Caching;

/// <summary>
/// Handler for deleting dashboard widget
/// </summary>
public class DeleteDashboardWidgetCommandHandler : IRequestHandler<DeleteDashboardWidgetCommand, DeleteWidgetResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeleteDashboardWidgetCommandHandler> _logger;

    public DeleteDashboardWidgetCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        ILogger<DeleteDashboardWidgetCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<DeleteWidgetResponse> Handle(
        DeleteDashboardWidgetCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting widget {WidgetId} from dashboard {DashboardId}", 
            command.WidgetId, command.DashboardId);

        try
        {
            // Get dashboard
            var dashboard = await _dashboardRepository.GetByIdAsync(command.DashboardId);
            if (dashboard == null)
            {
                throw new InvalidDashboardException($"Dashboard {command.DashboardId} not found");
            }

            // Find and remove widget
            var widget = dashboard.Widgets?.FirstOrDefault(w => w.Id == command.WidgetId);
            if (widget == null)
            {
                return new DeleteWidgetResponse(
                    Success: false,
                    Message: "Widget not found");
            }

            dashboard.Widgets!.Remove(widget);

            await _dashboardRepository.UpdateAsync(dashboard);

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");

            _logger.LogInformation("Widget deleted successfully: {WidgetId}", command.WidgetId);

            return new DeleteWidgetResponse(
                Success: true,
                Message: "Widget deleted successfully");
        }
        catch (InvalidDashboardException ex)
        {
            _logger.LogWarning(ex, "Dashboard not found: {DashboardId}", command.DashboardId);
            return new DeleteWidgetResponse(
                Success: false,
                Message: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting widget {WidgetId}", command.WidgetId);
            return new DeleteWidgetResponse(
                Success: false,
                Message: $"Failed to delete widget: {ex.Message}");
        }
    }
}
