using MediatR;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Queries;

/// <summary>
/// Query to get auth status.
/// </summary>
public class GetAuthStatusQuery : IRequest<object>
{
    public Guid UserId { get; set; }
}

