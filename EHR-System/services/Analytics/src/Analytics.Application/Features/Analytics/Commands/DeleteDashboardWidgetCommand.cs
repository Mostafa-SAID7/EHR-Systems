namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Command to delete dashboard widget
/// </summary>
public record DeleteDashboardWidgetCommand(
    Guid DashboardId,
    Guid WidgetId) : IRequest<DeleteWidgetResponse>;

/// <summary>
/// Response from deleting widget
/// </summary>
public record DeleteWidgetResponse(
    bool Success,
    string Message);
