using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>Get all dashboards for a user. Single Responsibility: Retrieve the user's full dashboard list with widgets.</summary>
public class GetUserDashboardsQueryHandler : IQueryHandler<GetUserDashboardsQuery, List<DashboardResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserDashboardsQueryHandler> _logger;
    public GetUserDashboardsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserDashboardsQueryHandler> logger) { _unitOfWork = unitOfWork; _logger = logger; }
    public async Task<List<DashboardResponseDto>> Handle(GetUserDashboardsQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Dashboard>();
        var dashboards = await repo.ToListAsync(q => q.Where(d => d.UserId == request.UserId), ct);
        return dashboards.Select(d => new DashboardResponseDto
        {
            Id = d.Id, UserId = d.UserId, Name = d.Name, Description = d.Description, IsDefault = d.IsDefault,
            Widgets = d.DashboardWidgets.Select(w => new DashboardWidgetDto { Id = w.Id, WidgetType = w.WidgetType, Title = w.Title, MetricName = w.MetricName }).ToList()
        }).ToList();
    }
}
