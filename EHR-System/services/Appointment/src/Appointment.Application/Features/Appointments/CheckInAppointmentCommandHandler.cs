using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.EventBus.Messaging;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

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



