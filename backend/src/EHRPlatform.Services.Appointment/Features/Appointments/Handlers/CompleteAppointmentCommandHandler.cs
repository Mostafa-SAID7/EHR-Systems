using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Infrastructure.EventDriven;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

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


