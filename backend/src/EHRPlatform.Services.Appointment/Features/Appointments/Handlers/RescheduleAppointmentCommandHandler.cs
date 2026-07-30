using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Reschedule appointment handler.
/// </summary>
public class RescheduleAppointmentCommandHandler : ICommandHandler<RescheduleAppointmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RescheduleAppointmentCommandHandler> _logger;

    public RescheduleAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RescheduleAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RescheduleAppointmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Rescheduling appointment {AppointmentId} from {OldTime} to {NewTime}",
            command.AppointmentId, DateTime.Now, command.NewScheduledStart);

        var repo = _unitOfWork.Repository<Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        // Validate availability (would check provider availability in real implementation)
        if (command.NewScheduledStart <= DateTime.UtcNow)
            throw new InvalidOperationException("New appointment time must be in the future");

        // Reschedule
        appointment.Reschedule(
            command.NewScheduledStart,
            command.DurationMinutes,
            command.InitiatedById,
            command.InitiatedBy,
            command.Reason);

        await repo.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {AppointmentId} rescheduled successfully to {NewTime}",
            command.AppointmentId, command.NewScheduledStart);
    }
}


