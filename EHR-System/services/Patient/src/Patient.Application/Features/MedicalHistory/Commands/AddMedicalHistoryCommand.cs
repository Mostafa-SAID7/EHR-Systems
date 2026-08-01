using MediatR;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Command to add medical history entry for patient.
/// Includes conditions, treatments, surgeries, hospitalizations.
/// </summary>
public record AddMedicalHistoryCommand : IRequest<AddMedicalHistoryResponse>
{
    public Guid PatientId { get; set; }
    public string HistoryType { get; set; } = string.Empty; // "Condition", "Treatment", "Surgery", "Hospitalization"
    public string Description { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public DateTime? OnsetDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Response after adding medical history
/// </summary>
public class AddMedicalHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? HistoryId { get; set; }
}
