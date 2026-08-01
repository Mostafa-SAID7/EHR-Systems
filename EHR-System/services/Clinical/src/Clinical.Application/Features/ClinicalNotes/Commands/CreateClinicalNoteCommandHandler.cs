using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Create clinical note handler.
/// Initializes SOAP format note with draft status.
/// Publishes ClinicalNoteCreatedEvent.
/// </summary>
public class CreateClinicalNoteCommandHandler : ICommandHandler<CreateClinicalNoteCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CreateClinicalNoteCommandHandler> _logger;

    public CreateClinicalNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CreateClinicalNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        CreateClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating clinical note for patient {PatientId} by provider {ProviderId}",
            command.PatientId, command.ProviderId);

        if (command.PatientId == Guid.Empty || command.ProviderId == Guid.Empty)
            throw new ArgumentException("PatientId and ProviderId cannot be empty");

        var clinicalNote = new Domain.ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = command.PatientId,
            ProviderId = command.ProviderId,
            Status = "Draft",
            Subjective = command.Subjective ?? string.Empty,
            Objective = command.Objective ?? string.Empty,
            Assessment = command.Assessment ?? string.Empty,
            Plan = command.Plan ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Vitals = new List<VitalSign>(),
            Diagnoses = new List<Diagnosis>(),
            Procedures = new List<Procedure>()
        };

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        await repository.AddAsync(clinicalNote, cancellationToken);

        // Publish event
        var createdEvent = new ClinicalNoteCreatedEvent(
            clinicalNote.Id,
            clinicalNote.PatientId,
            clinicalNote.ProviderId,
            clinicalNote.CreatedAt);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = clinicalNote.Id,
            EventType = nameof(ClinicalNoteCreatedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinical note created: {ClinicalNoteId}", clinicalNote.Id);

        return new ClinicalNoteResponse
        {
            Id = clinicalNote.Id,
            PatientId = clinicalNote.PatientId,
            ProviderId = clinicalNote.ProviderId,
            Status = clinicalNote.Status,
            Subjective = clinicalNote.Subjective,
            Objective = clinicalNote.Objective,
            Assessment = clinicalNote.Assessment,
            Plan = clinicalNote.Plan,
            CreatedAt = clinicalNote.CreatedAt,
            UpdatedAt = clinicalNote.UpdatedAt
        };
    }
}

/// <summary>
/// Domain event: Clinical note created
/// </summary>
public record ClinicalNoteCreatedEvent(Guid ClinicalNoteId, Guid PatientId, Guid ProviderId, DateTime CreatedAt);

/// <summary>
/// Domain models for Clinical service
/// </summary>
namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Domain
{
    public class ClinicalNote
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string Status { get; set; } = "Draft";
        public string Subjective { get; set; }
        public string Objective { get; set; }
        public string Assessment { get; set; }
        public string Plan { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<VitalSign> Vitals { get; set; } = new();
        public List<Diagnosis> Diagnoses { get; set; } = new();
        public List<Procedure> Procedures { get; set; } = new();

        public void AddVitalSign(VitalSign vital) => Vitals.Add(vital);
        public void AddDiagnosis(Diagnosis diagnosis) => Diagnoses.Add(diagnosis);
        public void AddProcedure(Procedure procedure) => Procedures.Add(procedure);
        public void Finalize() => Status = "Finalized";
    }

    public class VitalSign
    {
        public Guid Id { get; set; }
        public Guid ClinicalNoteId { get; set; }
        public string VitalType { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class Diagnosis
    {
        public Guid Id { get; set; }
        public Guid ClinicalNoteId { get; set; }
        public string ICD10Code { get; set; }
        public string Description { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class Procedure
    {
        public Guid Id { get; set; }
        public Guid ClinicalNoteId { get; set; }
        public string CPTCode { get; set; }
        public string Description { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
