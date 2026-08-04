namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Events;
using EHRPlatform.Services.Analytics.Domain.Exceptions;
using EHRPlatform.Services.Analytics.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.EventBus;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;
using EHRPlatform.BuildingBlocks.Security.CurrentUser;

/// <summary>
/// Handler for deleting dashboard with event publishing
/// </summary>
public class DeleteDashboardCommandHandler : IRequestHandler<DeleteDashboardCommand, DeleteDashboardResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly IMessageBroker _messageBroker;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteDashboardCommandHandler> _logger;

    public DeleteDashboardCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IMessageBroker messageBroker,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<DeleteDashboardCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _messageBroker = messageBroker;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
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

            var tenantId = _tenantContext.TenantId;
            var currentUserId = _currentUserService.GetUserId();

            // Delete dashboard
            await _dashboardRepository.DeleteAsync(command.DashboardId);

            // Publish DashboardDeletedEvent for audit trail
            var deletedEvent = new DashboardDeletedEvent(
                command.DashboardId,
                dashboard.Name,
                currentUserId,
                tenantId,
                DateTime.UtcNow);

            await _messageBroker.PublishAsync(deletedEvent, cancellationToken);

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
