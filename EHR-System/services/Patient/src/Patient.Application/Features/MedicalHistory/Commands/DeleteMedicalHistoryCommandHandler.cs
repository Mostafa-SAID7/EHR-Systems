using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Handler for DeleteMedicalHistoryCommand.
/// Soft deletes medical history entry.
/// </summary>
public class DeleteMedicalHistoryCommandHandler : IRequestHandler<DeleteMedicalHistoryCommand, DeleteMedicalHistoryResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<DeleteMedicalHistoryCommandHandler> _logger;

    public DeleteMedicalHistoryCommandHandler(
        IPatientDbContext context,
        ILogger<DeleteMedicalHistoryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DeleteMedicalHistoryResponse> Handle(
        DeleteMedicalHistoryCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting medical history {HistoryId} for patient {PatientId}",
            request.HistoryId, request.PatientId);

        try
        {
            var patient = await _context.Patients
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
                return new DeleteMedicalHistoryResponse { Success = false, Message = "Patient not found" };

            var historyEntry = patient.MedicalHistory?.FirstOrDefault(h => h.Id == request.HistoryId);
            if (historyEntry == null)
                return new DeleteMedicalHistoryResponse { Success = false, Message = "Medical history entry not found" };

            patient.RemoveMedicalHistoryEntry(historyEntry.Id);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical history entry deleted: {HistoryId}", request.HistoryId);

            return new DeleteMedicalHistoryResponse
            {
                Success = true,
                Message = "Medical history entry deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medical history {HistoryId}", request.HistoryId);
            return new DeleteMedicalHistoryResponse
            {
                Success = false,
                Message = "Error deleting medical history entry"
            };
        }
    }
}
