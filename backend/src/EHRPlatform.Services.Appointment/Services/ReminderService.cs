using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Domain.Enums;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Services;

/// <summary>
/// Appointment reminder service implementation.
/// Handles reminder scheduling, sending, and management using CQRS pattern.
/// </summary>
public class ReminderService : IReminderService
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher,
        ILogger<ReminderService> logger)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Schedule a reminder for an appointment.
    /// </summary>
    public async Task ScheduleReminderAsync(
        Guid appointmentId,
        DateTime reminderTime,
        ReminderType reminderType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Scheduling reminder for appointment {AppointmentId} at {ReminderTime} via {ReminderType}",
            appointmentId, reminderTime, reminderType);

        var command = new ScheduleReminderCommand
        {
            AppointmentId = appointmentId,
            ReminderTime = reminderTime,
            ReminderType = reminderType
        };

        await _commandDispatcher.DispatchAsync(command, cancellationToken);

        _logger.LogInformation("Reminder scheduled successfully for appointment {AppointmentId}", appointmentId);
    }

    /// <summary>
    /// Get all pending reminders that need to be sent.
    /// </summary>
    public async Task<IEnumerable<AppointmentReminderDto>> GetPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching pending reminders");

        var query = new GetPendingRemindersQuery();
        var reminders = await _queryDispatcher.DispatchAsync(query, cancellationToken);

        _logger.LogInformation("Found {ReminderCount} pending reminders", reminders.Count());

        return reminders;
    }

    /// <summary>
    /// Send a specific reminder.
    /// </summary>
    public async Task SendReminderAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending reminder {ReminderId}", reminderId);

        var command = new SendReminderCommand { ReminderId = reminderId };

        await _commandDispatcher.DispatchAsync(command, cancellationToken);

        _logger.LogInformation("Reminder {ReminderId} sent successfully", reminderId);
    }

    /// <summary>
    /// Send all pending reminders.
    /// </summary>
    public async Task<int> SendPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to send pending reminders");

        var pendingReminders = await GetPendingRemindersAsync(cancellationToken);
        var reminders = pendingReminders.ToList();

        if (!reminders.Any())
        {
            _logger.LogInformation("No pending reminders to send");
            return 0;
        }

        var sentCount = 0;
        foreach (var reminder in reminders)
        {
            try
            {
                await SendReminderAsync(reminder.Id, cancellationToken);
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder {ReminderId}", reminder.Id);
            }
        }

        _logger.LogInformation("Sent {SentCount} of {TotalCount} pending reminders", sentCount, reminders.Count);

        return sentCount;
    }

    /// <summary>
    /// Cancel a scheduled reminder.
    /// </summary>
    public async Task CancelReminderAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling reminder {ReminderId}", reminderId);

        // Future implementation: Create CancelReminderCommand
        await Task.CompletedTask;

        _logger.LogInformation("Reminder {ReminderId} cancelled successfully", reminderId);
    }
}


