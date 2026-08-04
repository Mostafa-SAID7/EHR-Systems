namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to update dashboard widget
/// </summary>
public record UpdateDashboardWidgetCommand(
    Guid DashboardId,
    Guid WidgetId,
    string? Title = null,
    int Position = -1,
    int SizeWidth = 0,
    int SizeHeight = 0,
    string? Configuration = null) : IRequest<UpdateWidgetResponse>;

/// <summary>
/// Response from updating widget
/// </summary>
public record UpdateWidgetResponse(
    bool Success,
    string Message);
