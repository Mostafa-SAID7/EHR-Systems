using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Schedule appointment reminder handler.
/// Creates an appointment reminder that will be sent at the specified time.
/// </summary>
public class ScheduleReminderCommandHandler : ICommandHandler<ScheduleReminderCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleReminderCommandHandler> _logger;

    public ScheduleReminderCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ScheduleReminderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ScheduleReminderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Scheduling reminder for appointment {AppointmentId} at {ReminderTime} via {ReminderType}",
            command.AppointmentId, command.ReminderTime, command.ReminderType);

        var repo = _unitOfWork.Repository<Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == command.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {command.AppointmentId} not found");

        // Create reminder
        var reminder = new AppointmentReminder
        {
            AppointmentId = appointment.Id,
            ReminderTime = command.ReminderTime,
            Method = command.ReminderType,
            Status = Domain.Enums.ReminderStatus.Scheduled,
            IsSent = false
        };

        // Add to appointment's reminders collection
        appointment.Reminders.Add(reminder);

        // Save
        await repo.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reminder scheduled successfully for appointment {AppointmentId}",
            command.AppointmentId);
    }
}
