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
/// Handler for updating dashboard with event publishing
/// </summary>
public class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand, UpdateDashboardResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly IMessageBroker _messageBroker;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateDashboardCommandHandler> _logger;

    public UpdateDashboardCommandHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IMessageBroker messageBroker,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<UpdateDashboardCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _messageBroker = messageBroker;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
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

            // Track changes for event
            string? updatedName = null;
            string? updatedDescription = null;
            bool? updatedIsPublic = null;

            // Update properties
            if (!string.IsNullOrWhiteSpace(command.Name) && command.Name != dashboard.Name)
            {
                updatedName = command.Name;
                dashboard.Name = command.Name;
            }

            if (!string.IsNullOrWhiteSpace(command.Description) && command.Description != dashboard.Description)
            {
                updatedDescription = command.Description;
                dashboard.Description = command.Description;
            }

            if (command.IsPublic.HasValue && command.IsPublic.Value != dashboard.IsPublic)
            {
                updatedIsPublic = command.IsPublic.Value;
                dashboard.IsPublic = command.IsPublic.Value;
            }

            dashboard.UpdatedAt = DateTime.UtcNow;

            // Save to repository
            await _dashboardRepository.UpdateAsync(dashboard);

            var tenantId = _tenantContext.TenantId;
            var currentUserId = _currentUserService.GetUserId();

            // Only publish event if something was actually changed
            if (updatedName != null || updatedDescription != null || updatedIsPublic.HasValue)
            {
                var updatedEvent = new DashboardUpdatedEvent(
                    command.DashboardId,
                    updatedName,
                    updatedDescription,
                    updatedIsPublic,
                    currentUserId,
                    tenantId,
                    DateTime.UtcNow);

                await _messageBroker.PublishAsync(updatedEvent, cancellationToken);
            }

            // Clear cache
            await _cacheService.RemoveAsync($"dashboard:{command.DashboardId}");
            await _cacheService.RemoveAsync("dashboards:all");
            await _cacheService.RemoveAsync($"kpi:summary:{tenantId}:{DateTime.UtcNow.Date:yyyyMMdd}");

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
