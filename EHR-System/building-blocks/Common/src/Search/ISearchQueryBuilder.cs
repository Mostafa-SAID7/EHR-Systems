using System;
using System.Collections.Generic;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Interface for building search queries fluently.
/// Single responsibility: Build and configure search queries.
/// </summary>
public interface ISearchQueryBuilder
{
    /// <summary>
    /// Set search text.
    /// </summary>
    ISearchQueryBuilder WithText(string text);

    /// <summary>
    /// Add field to search.
    /// </summary>
    ISearchQueryBuilder WithField(string fieldName);

    /// <summary>
    /// Add filter.
    /// </summary>
    ISearchQueryBuilder WithFilter(string field, string op, object value);

    /// <summary>
    /// Add sort.
    /// </summary>
    ISearchQueryBuilder WithSort(string field, string direction = "asc");

    /// <summary>
    /// Set pagination.
    /// </summary>
    ISearchQueryBuilder WithPagination(int skip, int take);

    /// <summary>
    /// Build search query.
    /// </summary>
    SearchQuery Build();
}
