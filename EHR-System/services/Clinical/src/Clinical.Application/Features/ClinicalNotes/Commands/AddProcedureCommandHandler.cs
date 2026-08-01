using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Add procedure handler.
/// Adds CPT/SNOMED code procedure to clinical note.
/// Publishes ProcedurePerformedEvent for subscriber services.
/// </summary>
public class AddProcedureCommandHandler : ICommandHandler<AddProcedureCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<AddProcedureCommandHandler> _logger;

    public AddProcedureCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<AddProcedureCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        AddProcedureCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding procedure to clinical note {ClinicalNoteId}: {CPTCode}",
            command.ClinicalNoteId, command.CPTCode);

        if (string.IsNullOrEmpty(command.CPTCode))
            throw new ArgumentException("CPTCode cannot be empty");

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        // Validate CPT format (5-digit code)
        if (!System.Text.RegularExpressions.Regex.IsMatch(command.CPTCode, @"^\d{5}$"))
            throw new ArgumentException($"Invalid CPT code format: {command.CPTCode}");

        var procedure = new Domain.Procedure
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = note.Id,
            CPTCode = command.CPTCode,
            Description = command.Description ?? string.Empty,
            PerformedAt = DateTime.UtcNow
        };

        note.AddProcedure(procedure);
        note.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(note, cancellationToken);

        // Publish event
        var procedureEvent = new ProcedurePerformedEvent(
            note.Id,
            note.PatientId,
            command.CPTCode,
            command.Description ?? string.Empty,
            DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(ProcedurePerformedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(procedureEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Procedure added to clinical note {ClinicalNoteId}", note.Id);

        return Unit.Value;
    }
}

/// <summary>
/// Domain event: Procedure performed
/// </summary>
public record ProcedurePerformedEvent(Guid ClinicalNoteId, Guid PatientId, string CPTCode, string Description, DateTime PerformedAt);
