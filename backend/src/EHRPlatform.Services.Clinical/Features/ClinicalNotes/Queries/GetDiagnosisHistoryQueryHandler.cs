using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get diagnosis history handler.
/// Single Responsibility: Retrieve ordered diagnosis history from clinical notes for a patient.
/// </summary>
public class GetDiagnosisHistoryQueryHandler : IQueryHandler<GetDiagnosisHistoryQuery, DiagnosisDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDiagnosisHistoryQueryHandler> _logger;

    public GetDiagnosisHistoryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDiagnosisHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DiagnosisDetailDto> Handle(
        GetDiagnosisHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching diagnosis history for patient {PatientId}", request.PatientId);

        var noteRepo = _unitOfWork.Repository<ClinicalNote>();
        var notes = await noteRepo.ToListAsync(
            q => q.Where(n => n.PatientId == request.PatientId)
                  .OrderByDescending(n => n.EncounterDate),
            cancellationToken);

        var diagnoses = notes
            .SelectMany(n => n.Diagnoses.Select(d => new DiagnosisHistoryItemDto
            {
                Id = d.Id,
                DiagnosisCode = d.DiagnosisCode,
                DiagnosisText = d.DiagnosisText,
                DiagnosisType = d.DiagnosisType,
                RecordedDate = n.EncounterDate,
                ClinicalNoteId = n.Id
            }))
            .OrderByDescending(d => d.RecordedDate)
            .ToList();

        return new DiagnosisDetailDto
        {
            PatientId = request.PatientId,
            Diagnoses = diagnoses
        };
    }
}

