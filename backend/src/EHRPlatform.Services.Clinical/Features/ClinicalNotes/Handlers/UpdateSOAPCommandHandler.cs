using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Update SOAP note handler.
/// </summary>
public class UpdateSOAPCommandHandler : ICommandHandler<UpdateSOAPCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSOAPCommandHandler> _logger;

    public UpdateSOAPCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateSOAPCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateSOAPCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating SOAP note {NoteId}", command.ClinicalNoteId);

        var repo = _unitOfWork.Repository<ClinicalNote>();
        var note = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == command.ClinicalNoteId),
            cancellationToken);

        if (note == null)
            throw new InvalidOperationException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status != "Draft")
            throw new InvalidOperationException("Only draft notes can be edited");

        if (!string.IsNullOrEmpty(command.Subjective))
            note.Subjective = command.Subjective;
        if (!string.IsNullOrEmpty(command.Objective))
            note.Objective = command.Objective;
        if (!string.IsNullOrEmpty(command.Assessment))
            note.Assessment = command.Assessment;
        if (!string.IsNullOrEmpty(command.Plan))
            note.Plan = command.Plan;

        await repo.UpdateAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


