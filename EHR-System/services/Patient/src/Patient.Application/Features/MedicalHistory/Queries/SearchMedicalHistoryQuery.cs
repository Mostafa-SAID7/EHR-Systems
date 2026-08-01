using MediatR;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Queries;

/// <summary>
/// Query to search medical history by condition/keyword.
/// Searches description and ICD-10 code.
/// </summary>
public record SearchMedicalHistoryQuery : IRequest<SearchMedicalHistoryResponse>
{
    public Guid PatientId { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response with search results
/// </summary>
public class SearchMedicalHistoryResponse
{
    public List<MedicalHistoryDto> Results { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
