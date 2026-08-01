using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Patient.Features.Patients.Commands;
using EHRPlatform.Services.Patient.Application.Patients.Responses;
using EHRPlatform.Services.Patient.Application.Patients.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Features.Patients.Handlers;

/// <summary>
/// Add patient allergy command handler.
/// Records allergy for patient with severity level.
/// </summary>
public class AddPatientAllergyCommandHandler : ICommandHandler<AddPatientAllergyCommand, PatientResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly PatientMapper _mapper;
    private readonly ILogger<AddPatientAllergyCommandHandler> _logger;

    public AddPatientAllergyCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        PatientMapper mapper,
        ILogger<AddPatientAllergyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PatientResponse> Handle(AddPatientAllergyCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding allergy {Allergen} for patient {PatientId}", command.Allergen, command.PatientId);

        var repo = _unitOfWork.Repository<PatientEntity>();
        var patient = await repo.GetByIdAsync(command.PatientId, cancellationToken);

        if (patient == null)
            throw new KeyNotFoundException($"Patient {command.PatientId} not found");

        patient.AddAllergy(command.Allergen, command.Severity, command.Notes ?? "");

        await repo.UpdateAsync(patient, cancellationToken);

        // Publish event
        var allergyEvent = new PatientAllergyAddedEvent(
            patient.Id, command.Allergen, command.Severity);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = patient.Id,
            EventType = nameof(PatientAllergyAddedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(allergyEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Allergy added for patient {PatientId}", patient.Id);

        return _mapper.MapToResponse(patient);
    }
}


