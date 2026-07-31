using EHRPlatform.Common.Application.Common.Behaviors;
using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.Search.Services;

namespace EHRPlatform.Common.Application.Features.Search.Queries;

// ─── Generic entity search ────────────────────────────────────────────────────

/// <summary>Generic search query, automatically cached via <see cref="CachingBehavior{,}"/>.</summary>
public sealed record SearchEntitiesQuery : IQuery<SearchResult<Dictionary<string, object>>>, ICachedQuery
{
    public string? QueryText  { get; init; }
    public int PageNumber     { get; init; } = 1;
    public int PageSize       { get; init; } = 10;
    public string EntityType  { get; init; } = "Patient";

    public string CacheKey  => $"search:{EntityType}:{QueryText}:{PageNumber}:{PageSize}".ToLower();
    public TimeSpan? Duration => TimeSpan.FromMinutes(10);
}

public sealed class SearchEntitiesQueryHandler
    : IQueryHandler<SearchEntitiesQuery, SearchResult<Dictionary<string, object>>>
{
    private readonly ISearchService _search;

    public SearchEntitiesQueryHandler(ISearchService search) =>
        _search = search ?? throw new ArgumentNullException(nameof(search));

    public Task<SearchResult<Dictionary<string, object>>> Handle(
        SearchEntitiesQuery query,
        CancellationToken cancellationToken)
    {
        var searchQuery = new SearchQuery
        {
            QueryText      = query.QueryText,
            PageNumber     = query.PageNumber,
            PageSize       = query.PageSize,
            HighlightResults = true
        };

        return _search.SearchAsync<Dictionary<string, object>>(searchQuery, cancellationToken);
    }
}

// ─── Patient search ───────────────────────────────────────────────────────────

public sealed record SearchPatientsQuery : IQuery<SearchResult<PatientSearchDto>>, ICachedQuery
{
    public string? SearchText { get; init; }
    public int PageNumber     { get; init; } = 1;
    public int PageSize       { get; init; } = 10;

    public string CacheKey  => $"patients:search:{SearchText}:{PageNumber}:{PageSize}".ToLower();
    public TimeSpan? Duration => TimeSpan.FromMinutes(10);
}

public sealed record PatientSearchDto
{
    public Guid   Id        { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName  { get; init; } = string.Empty;
    public string Email     { get; init; } = string.Empty;
    public string MRN       { get; init; } = string.Empty;
    public double Score     { get; init; }
}

public sealed class SearchPatientsQueryHandler
    : IQueryHandler<SearchPatientsQuery, SearchResult<PatientSearchDto>>
{
    private readonly ISearchService _search;

    public SearchPatientsQueryHandler(ISearchService search) =>
        _search = search ?? throw new ArgumentNullException(nameof(search));

    public Task<SearchResult<PatientSearchDto>> Handle(
        SearchPatientsQuery query,
        CancellationToken cancellationToken)
    {
        var searchQuery = new SearchQuery
        {
            QueryText  = query.SearchText,
            PageNumber = query.PageNumber,
            PageSize   = query.PageSize
        };

        return _search.SearchAsync<PatientSearchDto>(searchQuery, cancellationToken);
    }
}

// ─── SOAP notes search ────────────────────────────────────────────────────────

public sealed record SearchSoapNotesQuery : IQuery<SearchResult<SoapNoteSearchDto>>, ICachedQuery
{
    public string?   ClinicalText { get; init; }
    public Guid?     PatientId    { get; init; }
    public DateTime? StartDate    { get; init; }
    public DateTime? EndDate      { get; init; }
    public int PageNumber         { get; init; } = 1;
    public int PageSize           { get; init; } = 10;

    public string CacheKey  => $"soapnotes:search:{ClinicalText}:{PatientId}:{PageNumber}".ToLower();
    public TimeSpan? Duration => TimeSpan.FromMinutes(10);
}

public sealed record SoapNoteSearchDto
{
    public Guid     Id         { get; init; }
    public Guid     PatientId  { get; init; }
    public string   Assessment { get; init; } = string.Empty;
    public string   Plan       { get; init; } = string.Empty;
    public DateTime CreatedAt  { get; init; }
    public double   Score      { get; init; }
}

public sealed class SearchSoapNotesQueryHandler
    : IQueryHandler<SearchSoapNotesQuery, SearchResult<SoapNoteSearchDto>>
{
    private readonly ISearchService _search;

    public SearchSoapNotesQueryHandler(ISearchService search) =>
        _search = search ?? throw new ArgumentNullException(nameof(search));

    public Task<SearchResult<SoapNoteSearchDto>> Handle(
        SearchSoapNotesQuery query,
        CancellationToken cancellationToken)
    {
        var searchQuery = new SearchQuery
        {
            QueryText        = query.ClinicalText,
            PageNumber       = query.PageNumber,
            PageSize         = query.PageSize,
            DateRange        = (query.StartDate, query.EndDate),
            HighlightResults = true,
            SortBy           = new() { ("createdAt", SortOrder.Descending) }
        };

        return _search.SearchAsync<SoapNoteSearchDto>(searchQuery, cancellationToken);
    }
}

