namespace EHRPlatform.Services.Patient.Infrastructure.Services;

using EHRPlatform.Services.Patient.Application.Services;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Text.Json;

/// <summary>
/// Service for indexing and searching patients in Elasticsearch.
/// </summary>
public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string IndexPattern = "patients-{0:yyyy-MM}";

    public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexPatientAsync(string documentId, object document, CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = string.Format(IndexPattern, DateTime.UtcNow);
            var response = await _client.IndexAsync(
                new IndexRequest<object>(indexName)
                {
                    Id = documentId,
                    Document = document
                },
                cancellationToken);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Patient indexed successfully: {DocumentId}", documentId);
            }
            else
            {
                _logger.LogError("Failed to index patient: {DocumentId}, Error: {Error}", documentId, response.ApiCallDetails?.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing patient");
        }
    }

    public async Task<ElasticsearchSearchResult> SearchPatientsAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = string.Format(IndexPattern, DateTime.UtcNow);
            var from = (pageNumber - 1) * pageSize;

            var response = await _client.SearchAsync<PatientSearchDto>(s => s
                .Index(indexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(searchTerm)
                        .Fields(new[] { "firstName", "lastName", "email", "phone", "mrn" })
                        .Fuzziness(new Fuzziness(1)))),
                cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Search failed: {Error}", response.ApiCallDetails?.DebugInformation);
                return new ElasticsearchSearchResult();
            }

            var patients = response.Documents.ToList();
            var totalCount = response.Total;

            _logger.LogInformation("Search completed. Found {Count} patients", patients.Count);

            return new ElasticsearchSearchResult
            {
                Patients = patients,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients");
            return new ElasticsearchSearchResult();
        }
    }

    public async Task DeletePatientAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = string.Format(IndexPattern, DateTime.UtcNow);
            var response = await _client.DeleteAsync(indexName, documentId, ct: cancellationToken);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Patient deleted from index: {DocumentId}", documentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient from index");
        }
    }

    public async Task UpdatePatientAsync(string documentId, object document, CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = string.Format(IndexPattern, DateTime.UtcNow);
            var response = await _client.UpdateAsync<object, object>(
                indexName,
                documentId,
                u => u.Doc(document),
                cancellationToken);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Patient updated in index: {DocumentId}", documentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient in index");
        }
    }
}
