#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// Get users query response with pagination.
/// </summary>
public class GetUsersResponse
{
    /// <summary>
    /// Total number of users (before pagination).
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// List of users on this page.
    /// </summary>
    public List<UserResponseDto> Items { get; set; } = new();
}

