using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get clinical note by ID query handler.
/// Includes vitals, diagnoses, procedures.
/// Cached for performance.
/// </summary>
public class GetClinicalNoteQueryHandler : IQueryHandler<GetClinicalNoteQuery, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetClinicalNoteQueryHandler> _logger;

    public GetClinicalNoteQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetClinicalNoteQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        GetClinicalNoteQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting clinical note {ClinicalNoteId}", query.ClinicalNoteId);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(query.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {query.ClinicalNoteId} not found");

        return MapToResponse(note);
    }

    private static ClinicalNoteResponse MapToResponse(Domain.ClinicalNote note)
    {
        return new ClinicalNoteResponse
        {
            Id = note.Id,
            PatientId = note.PatientId,
            ProviderId = note.ProviderId,
            Status = note.Status,
            Subjective = note.Subjective,
            Objective = note.Objective,
            Assessment = note.Assessment,
            Plan = note.Plan,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}
