using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Update SOAP components handler.
/// Only available on draft notes.
/// Logs all changes for audit trail.
/// </summary>
public class UpdateSOAPCommandHandler : ICommandHandler<UpdateSOAPCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSOAPCommandHandler> _logger;

    public UpdateSOAPCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateSOAPCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        UpdateSOAPCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating SOAP for clinical note {ClinicalNoteId}",
            command.ClinicalNoteId);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status != "Draft")
            throw new InvalidOperationException("Can only update SOAP on draft notes");

        // Update SOAP components
        if (!string.IsNullOrEmpty(command.Subjective))
            note.Subjective = command.Subjective;

        if (!string.IsNullOrEmpty(command.Objective))
            note.Objective = command.Objective;

        if (!string.IsNullOrEmpty(command.Assessment))
            note.Assessment = command.Assessment;

        if (!string.IsNullOrEmpty(command.Plan))
            note.Plan = command.Plan;

        note.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SOAP updated for clinical note {ClinicalNoteId}", note.Id);

        return Unit.Value;
    }
}
