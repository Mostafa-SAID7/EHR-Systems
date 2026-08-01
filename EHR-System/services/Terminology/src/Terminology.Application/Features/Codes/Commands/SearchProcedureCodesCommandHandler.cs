namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for SearchProcedureCodesCommand - Searches procedure codes via Elasticsearch.
/// </summary>
public class SearchProcedureCodesCommandHandler : IRequestHandler<SearchProcedureCodesCommand, SearchProcedureCodesResponse>
{
    private readonly ICodeSearchService _searchService;
    private readonly ILogger<SearchProcedureCodesCommandHandler> _logger;

    public SearchProcedureCodesCommandHandler(
        ICodeSearchService searchService,
        ILogger<SearchProcedureCodesCommandHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<SearchProcedureCodesResponse> Handle(SearchProcedureCodesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching procedure codes: '{SearchTerm}' (Page {Page})", 
            request.SearchTerm, request.PageNumber);

        var results = await _searchService.SearchCodesAsync(
            "CPT",
            request.SearchTerm,
            request.SpecialtyFilter,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new SearchProcedureCodesResponse
        {
            Results = results.Codes.Select(c => new ProcedureCodeDto
            {
                CodeId = c.CodeId,
                Code = c.Code,
                Display = c.Display,
                Definition = c.Definition,
                UsageCount = c.UsageCount,
                RelevanceScore = c.RelevanceScore
            }).ToList(),
            TotalResults = results.TotalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
