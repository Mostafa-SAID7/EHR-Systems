using Mapster;
using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Appointment.Application.Appointments.Mappers;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

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


