namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for AutocompleteCodesQuery - Returns code suggestions.
/// </summary>
public class AutocompleteCodesQueryHandler : IRequestHandler<AutocompleteCodesQuery, AutocompleteCodesResponse>
{
    private readonly ICodeSearchService _searchService;
    private readonly ILogger<AutocompleteCodesQueryHandler> _logger;

    public AutocompleteCodesQueryHandler(
        ICodeSearchService searchService,
        ILogger<AutocompleteCodesQueryHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<AutocompleteCodesResponse> Handle(AutocompleteCodesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Autocomplete for {CodeSystem} with prefix '{Prefix}'", 
            request.CodeSystem, request.Prefix);

        var suggestions = await _searchService.AutocompleteAsync(
            request.CodeSystem,
            request.Prefix,
            request.MaxResults,
            cancellationToken);

        return new AutocompleteCodesResponse
        {
            Suggestions = suggestions
        };
    }
}
