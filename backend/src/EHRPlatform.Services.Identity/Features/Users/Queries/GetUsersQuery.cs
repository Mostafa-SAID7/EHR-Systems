#nullable enable

using EHRPlatform.Common.Application.Common.Behaviors;
using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Users.Queries;

/// <summary>
/// Query to retrieve paginated list of users.
/// Supports filtering by search term and active status.
/// Supports caching for improved performance.
/// </summary>
public class GetUsersQuery : IQuery<GetUsersResponse>, ICachedQuery
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100).
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional search term to filter users by email, first name, or last name.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional filter for active/inactive users.
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Cache key for this query result.
    /// Includes page number in key to cache different pages separately.
    /// </summary>
    public string CacheKey =>
        $"users_page_{PageNumber}_size_{PageSize}_search_{SearchTerm ?? "none"}_active_{IsActive?.ToString() ?? "any"}";

    /// <summary>
    /// Cache duration (300 seconds = 5 minutes).
    /// </summary>
    public TimeSpan? Duration => TimeSpan.FromSeconds(300);
}

