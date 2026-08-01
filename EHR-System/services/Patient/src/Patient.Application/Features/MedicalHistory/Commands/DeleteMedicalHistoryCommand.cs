using MediatR;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Command to delete medical history entry.
/// Performs soft delete (marks as deleted).
/// </summary>
public record DeleteMedicalHistoryCommand : IRequest<DeleteMedicalHistoryResponse>
{
    public Guid HistoryId { get; set; }
    public Guid PatientId { get; set; }
}

/// <summary>
/// Response after deleting medical history
/// </summary>
public class DeleteMedicalHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
