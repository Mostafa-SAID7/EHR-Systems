#nullable enable

using EHRPlatform.BuildingBlocks.EventBus.Behaviors;
using EHRPlatform.BuildingBlocks.EventBus.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Queries;

/// <summary>
/// Query to retrieve user permissions.
/// Flattens permissions from all user roles.
/// Supports caching for improved performance.
/// </summary>
public class GetUserPermissionsQuery : IQuery<GetUserPermissionsResponse>, ICachedQuery
{
    /// <summary>
    /// User ID to get permissions for.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Cache key for this query result.
    /// </summary>
    public string CacheKey => $"user_permissions_{UserId}";

    /// <summary>
    /// Cache duration (600 seconds = 10 minutes).
    /// Longer cache for permissions since they change less frequently.
    /// </summary>
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}


