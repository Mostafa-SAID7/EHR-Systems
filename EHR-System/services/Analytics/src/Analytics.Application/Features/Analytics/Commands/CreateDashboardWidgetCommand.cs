namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to create dashboard widget
/// </summary>
public record CreateDashboardWidgetCommand(
    Guid DashboardId,
    string Title,
    string WidgetType,
    int Position,
    int SizeWidth,
    int SizeHeight,
    string? Configuration = null) : IRequest<CreateWidgetResponse>;

/// <summary>
/// Response from creating widget
/// </summary>
public record CreateWidgetResponse(
    bool Success,
    string Message,
    Guid? WidgetId = null);
