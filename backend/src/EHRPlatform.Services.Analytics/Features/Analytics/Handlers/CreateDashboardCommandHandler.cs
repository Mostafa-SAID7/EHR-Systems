using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Implementations;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;
using Mapster;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

public class CreateDashboardCommandHandler : ICommandHandler<CreateDashboardCommand, DashboardResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDashboardCommandHandler> _logger;

    public CreateDashboardCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateDashboardCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DashboardResponse> Handle(CreateDashboardCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Creating dashboard for user {UserId}", command.UserId);
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(), 
            UserId = command.UserId,
            Name = command.Name, 
            Description = command.Description,
            IsDefault = command.IsDefault
        };
        var repo = _unitOfWork.Repository<Dashboard>();
        await repo.AddAsync(dashboard, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return dashboard.Adapt<DashboardResponse>();
    }
}


