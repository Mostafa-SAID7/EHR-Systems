using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;
// Domain entities via GlobalUsings (Domain.Entities)
using Mapster;

namespace EHRPlatform.Services.Patient.Features.Patients.Commands;

/// <summary>
/// Create patient command handler.
/// Generates MRN, publishes event to Kafka via outbox.
/// </summary>
public class CreatePatientCommandHandler : ICommandHandler<CreatePatientCommand, PatientResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CreatePatientCommandHandler> _logger;

    public CreatePatientCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CreatePatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<PatientResponseDto> Handle(
        CreatePatientCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating patient {FirstName} {LastName}", command.FirstName, command.LastName);

        var patientRepo = _unitOfWork.Repository<PatientEntity>();

        // Generate MRN (Medical Record Number)
        var mrn = GenerateMRN();

        var patient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            MRN = mrn,
            BloodType = command.BloodType,
            EmergencyContact = command.EmergencyContact,
            EmergencyPhone = command.EmergencyPhone,
            Status = "Active"
        };

        // Raise domain event
        var patientCreatedEvent = new PatientCreatedEvent(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.Email,
            patient.MRN);

        patient.RaiseEvent(patientCreatedEvent);

        // Add to repository
        await patientRepo.AddAsync(patient, cancellationToken);

        // Publish event via outbox for guaranteed delivery
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = patient.Id,
            EventType = nameof(PatientCreatedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(patientCreatedEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Patient created {PatientId} with MRN {MRN}", patient.Id, mrn);

        return patient.Adapt<PatientResponseDto>();
    }

    private string GenerateMRN()
    {
        // Format: P-YYYYMMDD-XXXXXX (P for patient, date, random)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"P-{timestamp}-{random}";
    }
}

/// <summary>
/// Update patient command handler.
/// </summary>
public class UpdatePatientCommandHandler : ICommandHandler<UpdatePatientCommand, PatientResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePatientCommandHandler> _logger;

    public UpdatePatientCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdatePatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PatientResponseDto> Handle(
        UpdatePatientCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating patient {PatientId}", command.PatientId);

        var patientRepo = _unitOfWork.Repository<PatientEntity>();
        var patient = await patientRepo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PatientId),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient {command.PatientId} not found");

        patient.FirstName = command.FirstName;
        patient.LastName = command.LastName;
        patient.Email = command.Email;
        patient.PhoneNumber = command.PhoneNumber;
        patient.BloodType = command.BloodType;
        patient.EmergencyContact = command.EmergencyContact;
        patient.EmergencyPhone = command.EmergencyPhone;

        await patientRepo.UpdateAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Adapt<PatientResponseDto>();
    }
}

/// <summary>
/// Add allergy command handler.
/// </summary>
public class AddAllergyCommandHandler : ICommandHandler<AddAllergyCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<AddAllergyCommandHandler> _logger;

    public AddAllergyCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<AddAllergyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(AddAllergyCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding allergy to patient {PatientId}", command.PatientId);

        var patientRepo = _unitOfWork.Repository<PatientEntity>();
        var patient = await patientRepo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PatientId),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient {command.PatientId} not found");

        patient.AddAllergy(command.Allergen, command.Severity, command.Notes);

        await patientRepo.UpdateAsync(patient, cancellationToken);

        // Publish event
        var allergyEvent = patient.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = patient.Id,
            EventType = nameof(PatientAllergyAddedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(allergyEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Add condition command handler.
/// </summary>
public class AddConditionCommandHandler : ICommandHandler<AddConditionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<AddConditionCommandHandler> _logger;

    public AddConditionCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<AddConditionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(AddConditionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding condition to patient {PatientId}", command.PatientId);

        var patientRepo = _unitOfWork.Repository<PatientEntity>();
        var patient = await patientRepo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PatientId),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient {command.PatientId} not found");

        patient.AddCondition(command.Condition, command.ICD10Code, command.OnsetDate);

        await patientRepo.UpdateAsync(patient, cancellationToken);

        // Publish event
        var conditionEvent = patient.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = patient.Id,
            EventType = nameof(PatientConditionAddedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(conditionEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
