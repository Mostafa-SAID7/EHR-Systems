namespace EHRPlatform.Services.Appointment.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Appointment.Controllers.Requests;
using EHRPlatform.Services.Appointment.Services;
using EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Appointment Reminders API - Schedule and manage reminders
/// Single Responsibility: Reminder management only
/// Separated from core appointments for independent scaling
/// </summary>
[ApiController]
[Route("api/v1/appointments/{appointmentId}/reminders")]
[Authorize]
public class AppointmentRemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<AppointmentRemindersController> _logger;

    public AppointmentRemindersController(
        IReminderService reminderService,
        ILogger<AppointmentRemindersController> logger)
    {
        _reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
        _logger = logger;
    }

    /// <summary>
    /// Schedule a reminder for an appointment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleReminder(
        Guid appointmentId,
        [FromBody] ScheduleReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Scheduling reminder for appointment {AppointmentId}: {ReminderType} at {ReminderTime}",
            appointmentId, request.ReminderType, request.ReminderTime);

        try
        {
            if (appointmentId == Guid.Empty)
                return BadRequest("AppointmentId cannot be empty");

            await _reminderService.ScheduleReminderAsync(
                appointmentId,
                request.ReminderTime,
                request.ReminderType,
                cancellationToken);

            _logger.LogInformation("Reminder scheduled successfully for appointment {AppointmentId}", appointmentId);

            return CreatedAtAction(nameof(GetPendingReminders), new { appointmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling reminder for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, "Error scheduling reminder");
        }
    }

    /// <summary>
    /// Get pending reminders for an appointment
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingReminders(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pending reminders for appointment {AppointmentId}", appointmentId);

        try
        {
            if (appointmentId == Guid.Empty)
                return BadRequest("AppointmentId cannot be empty");

            var reminders = await _reminderService.GetPendingRemindersAsync(cancellationToken);
            return Ok(reminders.Where(r => r.AppointmentId == appointmentId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminders for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, "Error retrieving reminders");
        }
    }

    /// <summary>
    /// Cancel a reminder
    /// </summary>
    [HttpDelete("{reminderId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReminder(
        Guid appointmentId,
        Guid reminderId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Canceling reminder {ReminderId} for appointment {AppointmentId}",
            reminderId, appointmentId);

        try
        {
            await _reminderService.CancelReminderAsync(reminderId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling reminder {ReminderId}", reminderId);
            return StatusCode(500, "Error canceling reminder");
        }
    }
}
