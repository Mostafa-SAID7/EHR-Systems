using Mapster;
using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Schedule appointment handler.
/// Validates provider availability, publishes event.
/// Delegates all mapping to AppointmentMapper (SRP).
/// </summary>
public class ScheduleAppointmentCommandHandler : ICommandHandler<ScheduleAppointmentCommand, AppointmentResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<ScheduleAppointmentCommandHandler> _logger;

    public ScheduleAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        AppointmentMapper mapper,
        ILogger<ScheduleAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppointmentResponseDto> Handle(
        ScheduleAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Scheduling appointment: Patient {PatientId}, Provider {ProviderId}, Start {Start}",
            command.PatientId, command.ProviderId, command.ScheduledStart);

        var availRepo = _unitOfWork.Repository<ProviderAvailability>();
        var availSlot = await availRepo.FirstOrDefaultAsync(
            q => q.Where(a =>
                a.ProviderId == command.ProviderId &&
                a.SlotStart <= command.ScheduledStart &&
                a.SlotEnd >= command.ScheduledStart.AddMinutes(command.DurationMinutes) &&
                a.IsActive),
            cancellationToken);

        if (availSlot == null || !availSlot.HasAvailability())
            throw new InvalidOperationException("Provider slot not available at requested time");

        var appointment = new Domain.Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = command.PatientId,
            ProviderId = command.ProviderId,
            ScheduledStart = command.ScheduledStart,
            ScheduledEnd = command.ScheduledStart.AddMinutes(command.DurationMinutes),
            AppointmentType = command.AppointmentType,
            Status = "Scheduled",
            ReasonForVisit = command.ReasonForVisit,
            Notes = command.Notes,
            DurationMinutes = command.DurationMinutes
        };

        // Add default reminders (24 hours, 2 hours before)
        appointment.AddReminder(command.ScheduledStart.AddHours(-24), "Email");
        appointment.AddReminder(command.ScheduledStart.AddHours(-2), "SMS");

        // Book the availability slot
        availSlot.BookSlot();

        var appointmentRepo = _unitOfWork.Repository<Domain.Appointment>();
        await appointmentRepo.AddAsync(appointment, cancellationToken);
        await availRepo.UpdateAsync(availSlot, cancellationToken);

        // Publish event
        var scheduledEvent = new AppointmentScheduledEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.ProviderId,
            appointment.ScheduledStart,
            appointment.AppointmentType);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = appointment.Id,
            EventType = nameof(AppointmentScheduledEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(scheduledEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Appointment scheduled {AppointmentId}", appointment.Id);

        return _mapper.MapToResponseDto(appointment);
    }
}

/// <summary>
/// Confirm appointment handler.
/// </summary>
public class ConfirmAppointmentCommandHandler : ICommandHandler<ConfirmAppointmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<ConfirmAppointmentCommandHandler> _logger;

    public ConfirmAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<ConfirmAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(ConfirmAppointmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming appointment {AppointmentId}", command.AppointmentId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        appointment.Confirm();
        await repo.UpdateAsync(appointment, cancellationToken);

        // Publish event
        var confirmEvent = new AppointmentConfirmedEvent(
            appointment.Id, appointment.PatientId, appointment.ProviderId, appointment.ScheduledStart);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = appointment.Id,
            EventType = nameof(AppointmentConfirmedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(confirmEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Cancel appointment handler.
/// </summary>
public class CancelAppointmentCommandHandler : ICommandHandler<CancelAppointmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CancelAppointmentCommandHandler> _logger;

    public CancelAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CancelAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(CancelAppointmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling appointment {AppointmentId}", command.AppointmentId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        appointment.Cancel(command.Reason);

        // Release availability slot
        var availRepo = _unitOfWork.Repository<ProviderAvailability>();
        var availSlot = await availRepo.FirstOrDefaultAsync(
            q => q.Where(a =>
                a.ProviderId == appointment.ProviderId &&
                a.SlotStart <= appointment.ScheduledStart &&
                a.SlotEnd >= appointment.ScheduledEnd),
            cancellationToken);

        if (availSlot != null)
        {
            availSlot.ReleaseSlot();
            await availRepo.UpdateAsync(availSlot, cancellationToken);
        }

        await repo.UpdateAsync(appointment, cancellationToken);

        // Publish event
        var cancelEvent = new AppointmentCancelledEvent(
            appointment.Id, appointment.PatientId, appointment.ProviderId, command.Reason);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = appointment.Id,
            EventType = nameof(AppointmentCancelledEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(cancelEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Check-in appointment handler.
/// </summary>
public class CheckInAppointmentCommandHandler : ICommandHandler<CheckInAppointmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CheckInAppointmentCommandHandler> _logger;

    public CheckInAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CheckInAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(CheckInAppointmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking in appointment {AppointmentId}", command.AppointmentId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        appointment.CheckIn();
        await repo.UpdateAsync(appointment, cancellationToken);

        // Publish event
        var checkInEvent = new AppointmentCheckedInEvent(
            appointment.Id, appointment.PatientId, appointment.ProviderId, DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = appointment.Id,
            EventType = nameof(AppointmentCheckedInEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(checkInEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Complete appointment handler.
/// </summary>
public class CompleteAppointmentCommandHandler : ICommandHandler<CompleteAppointmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CompleteAppointmentCommandHandler> _logger;

    public CompleteAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CompleteAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(CompleteAppointmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Completing appointment {AppointmentId}", command.AppointmentId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        appointment.Complete();
        await repo.UpdateAsync(appointment, cancellationToken);

        // Publish event
        var completeEvent = new AppointmentCompletedEvent(
            appointment.Id, appointment.PatientId, appointment.ProviderId, DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = appointment.Id,
            EventType = nameof(AppointmentCompletedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(completeEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Set provider availability handler.
/// Delegates ProviderAvailability mapping to AppointmentMapper (SRP).
/// </summary>
public class SetProviderAvailabilityCommandHandler : ICommandHandler<SetProviderAvailabilityCommand, ProviderAvailabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<SetProviderAvailabilityCommandHandler> _logger;

    public SetProviderAvailabilityCommandHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<SetProviderAvailabilityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProviderAvailabilityDto> Handle(
        SetProviderAvailabilityCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting availability for provider {ProviderId}: {Start} - {End}",
            command.ProviderId, command.SlotStart, command.SlotEnd);

        var availSlot = new ProviderAvailability
        {
            Id = Guid.NewGuid(),
            ProviderId = command.ProviderId,
            SlotStart = command.SlotStart,
            SlotEnd = command.SlotEnd,
            IsRecurring = command.IsRecurring,
            RecurrencePattern = command.RecurrencePattern,
            MaxAppointmentsPerSlot = command.MaxAppointmentsPerSlot,
            CurrentBookings = 0,
            IsActive = true
        };

        var repo = _unitOfWork.Repository<ProviderAvailability>();
        await repo.AddAsync(availSlot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return availSlot.Adapt<ProviderAvailabilityDto>();
    }
}
