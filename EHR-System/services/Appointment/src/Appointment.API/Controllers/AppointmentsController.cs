namespace EHRPlatform.Services.Appointment.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Application.Features.Appointments.Queries;
using EHRPlatform.Services.Appointment.Controllers.Requests;
using EHRPlatform.Services.Appointment.Services;
using EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Appointments API endpoints
/// </summary>
[ApiController]
[Route("api/v1/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReminderService? _reminderService;

    public AppointmentsController(IMediator mediator, IReminderService? reminderService = null)
    {
        _mediator = mediator;
        _reminderService = reminderService;
    }

    /// <summary>
    /// Schedule new appointment with conflict detection
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleAppointment([FromBody] ScheduleAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get appointment details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var result = await _mediator.Send(new GetAppointmentQuery { AppointmentId = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Confirm appointment
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmAppointment(Guid id)
    {
        var result = await _mediator.Send(new ConfirmAppointmentCommand { AppointmentId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "AppointmentService", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Schedule a reminder for an appointment.
    /// </summary>
    [HttpPost("{appointmentId}/reminders")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleReminder(
        Guid appointmentId,
        [FromBody] ScheduleReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_reminderService == null)
            return StatusCode(500, "Reminder service not available");

        await _reminderService.ScheduleReminderAsync(
            appointmentId,
            request.ReminderTime,
            request.ReminderType,
            cancellationToken);
        return CreatedAtAction(nameof(GetPendingReminders), null);
    }

    /// <summary>
    /// Add a note to an appointment.
    /// </summary>
    [HttpPost("{appointmentId}/notes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(
        Guid appointmentId,
        [FromBody] AddNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddNoteCommand
        {
            AppointmentId = appointmentId,
            Content = request.Content,
            CreatedById = request.CreatedById,
            PrivacyLevel = request.PrivacyLevel,
            Category = request.Category
        };

        await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAppointment), new { id = appointmentId });
    }

    /// <summary>
    /// Reschedule an appointment.
    /// </summary>
    [HttpPost("{appointmentId}/reschedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RescheduleAppointment(
        Guid appointmentId,
        [FromBody] RescheduleAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RescheduleAppointmentCommand
        {
            AppointmentId = appointmentId,
            NewScheduledStart = request.NewScheduledStart,
            DurationMinutes = request.DurationMinutes,
            InitiatedById = request.InitiatedById,
            InitiatedBy = request.InitiatedBy ?? "Provider",
            Reason = request.Reason
        };

        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Gets pending reminders that need to be sent.
    /// </summary>
    [HttpGet("reminders/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReminders(CancellationToken cancellationToken = default)
    {
        if (_reminderService == null)
            return StatusCode(500, "Reminder service not available");

        var reminders = await _reminderService.GetPendingRemindersAsync(cancellationToken);
        return Ok(reminders);
    }
}
