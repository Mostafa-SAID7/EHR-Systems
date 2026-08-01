namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;

/// <summary>
/// Delete dashboard
/// </summary>
public record DeleteDashboardCommand(
    Guid DashboardId) : IRequest<DeleteDashboardResponse>;

/// <summary>
/// Response from deleting dashboard
/// </summary>
public record DeleteDashboardResponse(
    bool Success,
    string Message);
