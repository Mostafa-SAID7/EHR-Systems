using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Infrastructure.EventDriven;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

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

