namespace EHRPlatform.Services.Patient.Application.Services;

/// <summary>
/// Service for indexing and searching patients in Elasticsearch.
/// </summary>
public interface IElasticsearchService
{
    /// <summary>
    /// Index a patient document in Elasticsearch.
    /// </summary>
    Task IndexPatientAsync(string documentId, object document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search patients by full-text query with pagination.
    /// </summary>
    Task<ElasticsearchSearchResult> SearchPatientsAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete patient document from Elasticsearch.
    /// </summary>
    Task DeletePatientAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update patient document in Elasticsearch.
    /// </summary>
    Task UpdatePatientAsync(string documentId, object document, CancellationToken cancellationToken = default);
}

public class ElasticsearchSearchResult
{
    public List<PatientSearchDto> Patients { get; set; } = new();
    public long TotalCount { get; set; }
}

public class PatientSearchDto
{
    public Guid Id { get; set; }
    public string Mrn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
}
