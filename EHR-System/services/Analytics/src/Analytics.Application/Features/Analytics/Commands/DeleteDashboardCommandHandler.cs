namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.Services.Analytics.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.EventBus;

/// <summary>
/// Handler for deleting dashboard
/// </summary>
public class DeleteDashboardCommandHandler : IRequestHandler<DeleteDashboardCommand, DeleteDashboardResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<DeleteDashboardCommandHandler> _logger;

    public DeleteDashboardCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IMessageBroker messageBroker,
        ILogger<DeleteDashboardCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task<DeleteDashboardResponse> Handle(
        DeleteDashboardCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting dashboard {DashboardId}", command.DashboardId);

        try
        {
            // Validate dashboard exists
            var dashboard = await _dashboardRepository.GetByIdAsync(command.DashboardId);
            if (dashboard == null)
            {
                throw new InvalidDashboardException($"Dashboard {command.DashboardId} not found");
            }

            // Delete dashboard
            await _dashboardRepository.DeleteAsync(command.DashboardId);

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");
            await _cacheService.RemoveAsync("dashboards:all");

            _logger.LogInformation("Dashboard deleted successfully: {DashboardId}", command.DashboardId);

            return new DeleteDashboardResponse(
                Success: true,
                Message: "Dashboard deleted successfully");
        }
        catch (InvalidDashboardException ex)
        {
            _logger.LogWarning(ex, "Validation error deleting dashboard {DashboardId}", command.DashboardId);
            return new DeleteDashboardResponse(
                Success: false,
                Message: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dashboard {DashboardId}", command.DashboardId);
            return new DeleteDashboardResponse(
                Success: false,
                Message: $"Failed to delete dashboard: {ex.Message}");
        }
    }
}
