#nullable enable

using EHRPlatform.Common.Application.Behaviors;
using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Queries;

/// <summary>
/// Query to retrieve user by ID.
/// Supports caching for improved performance.
/// </summary>
public class GetUserByIdQuery : IQuery<UserResponseDto>, ICachedQuery
{
    /// <summary>
    /// User ID to retrieve.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Cache key for this query result.
    /// </summary>
    public string CacheKey => $"user_{UserId}";

    /// <summary>
    /// Cache duration (300 seconds = 5 minutes).
    /// </summary>
    public TimeSpan? Duration => TimeSpan.FromSeconds(300);
}

