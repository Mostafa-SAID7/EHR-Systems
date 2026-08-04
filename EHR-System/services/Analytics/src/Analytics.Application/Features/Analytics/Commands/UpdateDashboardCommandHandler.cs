namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.Services.Analytics.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.EventBus;

/// <summary>
/// Handler for updating dashboard
/// </summary>
public class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand, UpdateDashboardResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<UpdateDashboardCommandHandler> _logger;

    public UpdateDashboardCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IMessageBroker messageBroker,
        ILogger<UpdateDashboardCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task<UpdateDashboardResponse> Handle(
        UpdateDashboardCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating dashboard {DashboardId}", command.DashboardId);

        try
        {
            // Validate dashboard exists
            var dashboard = await _dashboardRepository.GetByIdAsync(command.DashboardId);
            if (dashboard == null)
            {
                throw new InvalidDashboardException($"Dashboard {command.DashboardId} not found");
            }

            // Update properties
            if (!string.IsNullOrWhiteSpace(command.Name))
                dashboard.Name = command.Name;

            if (!string.IsNullOrWhiteSpace(command.Description))
                dashboard.Description = command.Description;

            if (command.IsPublic.HasValue)
                dashboard.IsPublic = command.IsPublic.Value;

            dashboard.UpdatedAt = DateTime.UtcNow;

            // Save to repository
            await _dashboardRepository.UpdateAsync(dashboard);

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");

            _logger.LogInformation("Dashboard updated successfully: {DashboardId}", command.DashboardId);

            return new UpdateDashboardResponse
            {
                Success = true,
                Message = "Dashboard updated successfully",
                DashboardId = command.DashboardId
            };
        }
        catch (InvalidDashboardException ex)
        {
            _logger.LogWarning(ex, "Validation error updating dashboard {DashboardId}", command.DashboardId);
            return new UpdateDashboardResponse
            {
                Success = false,
                Message = ex.Message,
                DashboardId = command.DashboardId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dashboard {DashboardId}", command.DashboardId);
            return new UpdateDashboardResponse
            {
                Success = false,
                Message = $"Failed to update dashboard: {ex.Message}",
                DashboardId = command.DashboardId
            };
        }
    }
}
