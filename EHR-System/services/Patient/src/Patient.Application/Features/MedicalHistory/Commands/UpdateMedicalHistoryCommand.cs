using MediatR;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Command to update medical history entry.
/// </summary>
public record UpdateMedicalHistoryCommand : IRequest<UpdateMedicalHistoryResponse>
{
    public Guid HistoryId { get; set; }
    public Guid PatientId { get; set; }
    public string? Description { get; set; }
    public DateTime? OnsetDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Response after updating medical history
/// </summary>
public class UpdateMedicalHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
