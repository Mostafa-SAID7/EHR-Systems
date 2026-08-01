using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Send appointment reminder handler.
/// Sends a scheduled reminder notification and marks it as sent.
/// </summary>
public class SendReminderCommandHandler : ICommandHandler<SendReminderCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendReminderCommandHandler> _logger;

    public SendReminderCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SendReminderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(SendReminderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending reminder {ReminderId}", command.ReminderId);

        var reminderRepo = _unitOfWork.Repository<AppointmentReminder>();
        var reminder = await reminderRepo.FirstOrDefaultAsync(
            q => q.Where(r => r.Id == command.ReminderId),
            cancellationToken);

        if (reminder == null)
            throw new InvalidOperationException($"Reminder {command.ReminderId} not found");

        if (reminder.IsSent)
            throw new InvalidOperationException($"Reminder {command.ReminderId} has already been sent");

        // Mark as sent
        reminder.IsSent = true;
        reminder.SentAt = DateTime.UtcNow;
        reminder.Status = Domain.Enums.ReminderStatus.Sent;

        await reminderRepo.UpdateAsync(reminder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reminder {ReminderId} sent successfully", command.ReminderId);
    }
}



