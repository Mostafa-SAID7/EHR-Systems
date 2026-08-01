namespace EHRPlatform.Contracts.Requests;

/// <summary>
/// Base create request model.
/// </summary>
public abstract class CreateRequest
{
}

/// <summary>
/// Base update request model.
/// </summary>
public abstract class UpdateRequest
{
}

/// <summary>
/// Base filter/search request.
/// </summary>
public abstract class SearchRequest
{
    /// <summary>
    /// Page number (1-indexed). Default: 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default: 10, Max: 100.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field (e.g., "createdAt", "-updatedAt" for desc).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Search keyword (optional full-text search).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Validate pagination parameters.
    /// </summary>
    public bool Validate()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 10;

        if (PageSize > 100)
            PageSize = 100;

        return true;
    }
}
