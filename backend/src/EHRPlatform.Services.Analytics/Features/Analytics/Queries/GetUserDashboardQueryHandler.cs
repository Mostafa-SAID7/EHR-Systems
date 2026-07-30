using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>Get a single user dashboard by ID. Single Responsibility: Fetch one dashboard with its widgets.</summary>
public class GetUserDashboardQueryHandler : IQueryHandler<GetUserDashboardQuery, DashboardResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserDashboardQueryHandler> _logger;
    public GetUserDashboardQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserDashboardQueryHandler> logger) { _unitOfWork = unitOfWork; _logger = logger; }
    public async Task<DashboardResponseDto> Handle(GetUserDashboardQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Dashboard>();
        var dashboard = await repo.FirstOrDefaultAsync(q => q.Where(d => d.Id == request.DashboardId && d.UserId == request.UserId), ct)
            ?? throw new InvalidOperationException($"Dashboard {request.DashboardId} not found");
        return new DashboardResponseDto
        {
            Id = dashboard.Id, UserId = dashboard.UserId, Name = dashboard.Name, Description = dashboard.Description, IsDefault = dashboard.IsDefault,
            Widgets = dashboard.DashboardWidgets.Select(w => new DashboardWidgetDto { Id = w.Id, WidgetType = w.WidgetType, Title = w.Title, MetricName = w.MetricName }).ToList()
        };
    }
}

