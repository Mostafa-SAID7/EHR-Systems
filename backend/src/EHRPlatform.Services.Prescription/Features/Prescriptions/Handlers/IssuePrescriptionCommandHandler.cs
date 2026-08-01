using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Mapster;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Issue prescription handler.
/// </summary>
public class IssuePrescriptionCommandHandler : ICommandHandler<IssuePrescriptionCommand, PrescriptionResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<IssuePrescriptionCommandHandler> _logger;

    public IssuePrescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<IssuePrescriptionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto> Handle(
        IssuePrescriptionCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Issuing prescription: Patient {PatientId}, Provider {ProviderId}, Medication {Med}",
            command.PatientId, command.ProviderId, command.MedicationName);

        var prescription = new PrescriptionEntity {
            Id = Guid.NewGuid(),
            PatientId = command.PatientId,
            ProviderId = command.ProviderId,
            MedicationName = command.MedicationName,
            Strength = command.Strength,
            FormType = command.FormType,
            Dosage = command.Dosage,
            Frequency = command.Frequency,
            Quantity = command.Quantity,
            RefillsAllowed = command.RefillsAllowed,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Indications = command.Indications,
            SpecialInstructions = command.SpecialInstructions,
            IsControlledSubstance = command.IsControlledSubstance,
            NDCCode = command.NDCCode
        };

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        await repo.AddAsync(prescription, cancellationToken);

        // Publish event
        var issuedEvent = new PrescriptionIssuedEvent(
            prescription.Id, prescription.PatientId, prescription.ProviderId,
            prescription.MedicationName, prescription.Dosage);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionIssuedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(issuedEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Prescription issued {PrescriptionId}", prescription.Id);

        return prescription.Adapt<PrescriptionResponseDto>();
    }
}



