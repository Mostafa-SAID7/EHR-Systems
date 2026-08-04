namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;

/// <summary>
/// Query to get all dashboards for current user/tenant
/// </summary>
public class GetDashboardsQuery : IRequest<GetDashboardsResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool IncludeArchived { get; set; } = false;
}

/// <summary>
/// Response with list of dashboards
/// </summary>
public class GetDashboardsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<DashboardListItemDto> Dashboards { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class DashboardListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int WidgetCount { get; set; }
}
