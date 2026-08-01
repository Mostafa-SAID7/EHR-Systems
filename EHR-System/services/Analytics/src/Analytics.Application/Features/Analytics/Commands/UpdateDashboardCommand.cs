namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Update dashboard configuration
/// </summary>
public record UpdateDashboardCommand(
    Guid DashboardId,
    string? Name = null,
    string? Description = null,
    object? Configuration = null) : IRequest<UpdateDashboardResponse>;

/// <summary>
/// Response from updating dashboard
/// </summary>
public record UpdateDashboardResponse(
    bool Success,
    string Message,
    Guid? DashboardId = null);
