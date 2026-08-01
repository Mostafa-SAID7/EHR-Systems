namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for SearchDiagnosisCodesCommand - Searches diagnosis codes via Elasticsearch.
/// </summary>
public class SearchDiagnosisCodesCommandHandler : IRequestHandler<SearchDiagnosisCodesCommand, SearchDiagnosisCodesResponse>
{
    private readonly ICodeSearchService _searchService;
    private readonly ILogger<SearchDiagnosisCodesCommandHandler> _logger;

    public SearchDiagnosisCodesCommandHandler(
        ICodeSearchService searchService,
        ILogger<SearchDiagnosisCodesCommandHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<SearchDiagnosisCodesResponse> Handle(SearchDiagnosisCodesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching diagnosis codes: '{SearchTerm}' (Page {Page})", 
            request.SearchTerm, request.PageNumber);

        var results = await _searchService.SearchCodesAsync(
            "ICD-10",
            request.SearchTerm,
            request.Category,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new SearchDiagnosisCodesResponse
        {
            Results = results.Codes,
            TotalResults = results.TotalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
