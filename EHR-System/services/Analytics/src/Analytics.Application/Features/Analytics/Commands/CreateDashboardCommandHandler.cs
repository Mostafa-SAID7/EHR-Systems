namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Domain.Events;
using EHRPlatform.Services.Analytics.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;
using EHRPlatform.BuildingBlocks.Security.CurrentUser;
using EHRPlatform.BuildingBlocks.EventBus;

/// <summary>
/// Handler for creating dashboard with event publishing
/// </summary>
public class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, CreateDashboardResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<CreateDashboardCommandHandler> _logger;

    public CreateDashboardCommandHandler(
        IDashboardRepository dashboardRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IMessageBroker messageBroker,
        ILogger<CreateDashboardCommandHandler> logger)
    {
        _dashboardRepository = dashboardRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task<CreateDashboardResponse> Handle(
        CreateDashboardCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating dashboard {DashboardName} for user {UserId}", command.Name, command.UserId);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new CreateDashboardResponse
                {
                    Success = false,
                    Message = "Tenant context not available"
                };
            }

            // Create new dashboard entity
            var dashboard = new Dashboard
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Description = command.Description,
                CreatedBy = command.UserId,
                IsPublic = command.IsPublic,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            // Save to repository
            var savedDashboard = await _dashboardRepository.AddAsync(dashboard);

            // Publish DashboardCreatedEvent for event sourcing and notifications
            var createdEvent = new DashboardCreatedEvent(
                savedDashboard.Id,
                savedDashboard.Name,
                savedDashboard.CreatedBy,
                tenantId,
                savedDashboard.CreatedAt);

            await _messageBroker.PublishAsync(createdEvent, cancellationToken);

            _logger.LogInformation("Dashboard created successfully: {DashboardId}", savedDashboard.Id);

            return new CreateDashboardResponse
            {
                Success = true,
                Message = "Dashboard created successfully",
                DashboardId = savedDashboard.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dashboard {DashboardName}", command.Name);
            return new CreateDashboardResponse
            {
                Success = false,
                Message = $"Failed to create dashboard: {ex.Message}"
            };
        }
    }
}
