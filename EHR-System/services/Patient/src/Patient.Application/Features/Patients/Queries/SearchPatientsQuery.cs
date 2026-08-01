namespace EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

using MediatR;

/// <summary>
/// Query to search patients using Elasticsearch full-text search.
/// </summary>
public class SearchPatientsQuery : IRequest<SearchPatientsResponse>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchPatientsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PatientDto> Patients { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
