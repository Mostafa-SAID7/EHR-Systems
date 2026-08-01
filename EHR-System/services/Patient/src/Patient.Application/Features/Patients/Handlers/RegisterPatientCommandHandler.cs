using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;
using EHRPlatform.Services.Patient.Application.Patients.Responses;
using EHRPlatform.Services.Patient.Application.Patients.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Handlers;

/// <summary>
/// Register patient command handler.
/// Creates new patient with initial data validation.
/// </summary>
public class RegisterPatientCommandHandler : ICommandHandler<RegisterPatientCommand, PatientResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly PatientMapper _mapper;
    private readonly ILogger<RegisterPatientCommandHandler> _logger;

    public RegisterPatientCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        PatientMapper mapper,
        ILogger<RegisterPatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PatientResponse> Handle(RegisterPatientCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering patient {Email}", command.Email);

        var patient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            MRN = command.MRN,
            BloodType = command.BloodType,
            EmergencyContact = command.EmergencyContact,
            EmergencyPhone = command.EmergencyPhone,
            Status = "Active"
        };

        var repo = _unitOfWork.Repository<PatientEntity>();
        await repo.AddAsync(patient, cancellationToken);

        // Publish event
        var registeredEvent = new PatientRegisteredEvent(
            patient.Id, patient.FirstName, patient.LastName, patient.Email, patient.MRN);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = patient.Id,
            EventType = nameof(PatientRegisteredEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(registeredEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Patient registered {PatientId}", patient.Id);

        return _mapper.MapToResponse(patient);
    }
}



