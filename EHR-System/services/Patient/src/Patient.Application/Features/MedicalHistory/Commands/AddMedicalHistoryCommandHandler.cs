using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;

/// <summary>
/// Handler for AddMedicalHistoryCommand.
/// Adds medical history entry to patient record.
/// </summary>
public class AddMedicalHistoryCommandHandler : IRequestHandler<AddMedicalHistoryCommand, AddMedicalHistoryResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<AddMedicalHistoryCommandHandler> _logger;

    public AddMedicalHistoryCommandHandler(
        IPatientDbContext context,
        ILogger<AddMedicalHistoryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AddMedicalHistoryResponse> Handle(
        AddMedicalHistoryCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding medical history to patient {PatientId}: {HistoryType}",
            request.PatientId, request.HistoryType);

        try
        {
            if (request.PatientId == Guid.Empty)
                return new AddMedicalHistoryResponse { Success = false, Message = "PatientId is required" };

            var patient = await _context.Patients
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
                return new AddMedicalHistoryResponse { Success = false, Message = "Patient not found" };

            var historyEntry = new Domain.MedicalHistoryEntry
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                HistoryType = request.HistoryType,
                Description = request.Description,
                ICD10Code = request.ICD10Code,
                OnsetDate = request.OnsetDate,
                ResolvedDate = request.ResolvedDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            patient.AddMedicalHistoryEntry(historyEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical history entry added: {HistoryId}", historyEntry.Id);

            return new AddMedicalHistoryResponse
            {
                Success = true,
                Message = "Medical history entry added successfully",
                HistoryId = historyEntry.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medical history for patient {PatientId}", request.PatientId);
            return new AddMedicalHistoryResponse
            {
                Success = false,
                Message = "Error adding medical history entry"
            };
        }
    }
}

/// <summary>
/// Domain entity for medical history
/// </summary>
namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Domain
{
    public class MedicalHistoryEntry
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string HistoryType { get; set; } // "Condition", "Treatment", "Surgery", "Hospitalization"
        public string Description { get; set; }
        public string? ICD10Code { get; set; }
        public DateTime? OnsetDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
