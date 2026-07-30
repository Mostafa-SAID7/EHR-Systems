using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

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

