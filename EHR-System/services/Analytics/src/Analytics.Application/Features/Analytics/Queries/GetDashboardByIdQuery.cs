namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;

/// <summary>
/// Query to get dashboard by ID
/// </summary>
public class GetDashboardByIdQuery : IRequest<GetDashboardByIdResponse>
{
    public Guid DashboardId { get; set; }
}

/// <summary>
/// Response with dashboard details
/// </summary>
public class GetDashboardByIdResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DashboardDetailDto? Dashboard { get; set; }
}

public class DashboardDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public List<WidgetDto> Widgets { get; set; } = new();
}

public class WidgetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public int Position { get; set; }
    public int SizeWidth { get; set; }
    public int SizeHeight { get; set; }
}
