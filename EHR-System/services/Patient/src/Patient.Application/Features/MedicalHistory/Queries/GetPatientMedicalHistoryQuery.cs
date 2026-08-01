using MediatR;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Queries;

/// <summary>
/// Query to get all medical history entries for patient.
/// Paginated, filterable by type.
/// </summary>
public record GetPatientMedicalHistoryQuery : IRequest<GetPatientMedicalHistoryResponse>
{
    public Guid PatientId { get; set; }
    public string? HistoryType { get; set; } // Optional filter
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response with medical history entries
/// </summary>
public class GetPatientMedicalHistoryResponse
{
    public List<MedicalHistoryDto> Entries { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Medical history DTO
/// </summary>
public class MedicalHistoryDto
{
    public Guid Id { get; set; }
    public string HistoryType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public DateTime? OnsetDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
