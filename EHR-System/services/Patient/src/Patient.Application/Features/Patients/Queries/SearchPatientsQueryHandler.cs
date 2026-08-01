namespace EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

using MediatR;
using EHRPlatform.Services.Patient.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for SearchPatientsQuery - Full-text search using Elasticsearch.
/// </summary>
public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, SearchPatientsResponse>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<SearchPatientsQueryHandler> _logger;

    public SearchPatientsQueryHandler(
        IElasticsearchService elasticsearchService,
        ILogger<SearchPatientsQueryHandler> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task<SearchPatientsResponse> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching patients: {SearchTerm}, Page: {PageNumber}", request.SearchTerm, request.PageNumber);

        try
        {
            var result = await _elasticsearchService.SearchPatientsAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return new SearchPatientsResponse
            {
                Success = true,
                Patients = result.Patients,
                TotalCount = result.TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients");
            return new SearchPatientsResponse
            {
                Success = false,
                Message = "An error occurred while searching patients"
            };
        }
    }
}
