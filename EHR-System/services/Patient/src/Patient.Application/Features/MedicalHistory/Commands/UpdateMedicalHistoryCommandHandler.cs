using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Handler for UpdateMedicalHistoryCommand.
/// Updates medical history entry.
/// </summary>
public class UpdateMedicalHistoryCommandHandler : IRequestHandler<UpdateMedicalHistoryCommand, UpdateMedicalHistoryResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<UpdateMedicalHistoryCommandHandler> _logger;

    public UpdateMedicalHistoryCommandHandler(
        IPatientDbContext context,
        ILogger<UpdateMedicalHistoryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpdateMedicalHistoryResponse> Handle(
        UpdateMedicalHistoryCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating medical history {HistoryId} for patient {PatientId}",
            request.HistoryId, request.PatientId);

        try
        {
            var patient = await _context.Patients
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
                return new UpdateMedicalHistoryResponse { Success = false, Message = "Patient not found" };

            var historyEntry = patient.MedicalHistory?.FirstOrDefault(h => h.Id == request.HistoryId);
            if (historyEntry == null)
                return new UpdateMedicalHistoryResponse { Success = false, Message = "Medical history entry not found" };

            if (!string.IsNullOrEmpty(request.Description))
                historyEntry.Description = request.Description;

            if (request.OnsetDate.HasValue)
                historyEntry.OnsetDate = request.OnsetDate;

            if (request.ResolvedDate.HasValue)
                historyEntry.ResolvedDate = request.ResolvedDate;

            if (!string.IsNullOrEmpty(request.Notes))
                historyEntry.Notes = request.Notes;

            historyEntry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical history entry updated: {HistoryId}", request.HistoryId);

            return new UpdateMedicalHistoryResponse
            {
                Success = true,
                Message = "Medical history entry updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medical history {HistoryId}", request.HistoryId);
            return new UpdateMedicalHistoryResponse
            {
                Success = false,
                Message = "Error updating medical history entry"
            };
        }
    }
}
