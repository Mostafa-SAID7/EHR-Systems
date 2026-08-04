namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using EHRPlatform.Services.Analytics.Contracts.Responses;

/// <summary>
/// Command to create new dashboard.
/// </summary>
public class CreateDashboardCommand : IRequest<CreateDashboardResponse>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
}

