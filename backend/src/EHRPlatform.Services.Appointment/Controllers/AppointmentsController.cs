using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Controllers;

/// <summary>
/// Manages appointment scheduling, status transitions, and lifecycle.
/// Entities: Appointment aggregate with reminder support.
/// </summary>
[ApiController]
[Route("api/v1/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IMediator mediator,
        ILogger<AppointmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a new appointment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleAppointment(
        [FromBody] ScheduleAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAppointment), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets appointment by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentQuery { AppointmentId = id };
        var result = await _mediator.Send(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Gets appointments for a patient with optional date range and pagination.
    /// </summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientAppointments(
        Guid patientId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientAppointmentsQuery
        {
            PatientId = patientId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirms an appointment.
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmAppointment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ConfirmAppointmentCommand { AppointmentId = id };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Cancels an appointment.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAppointment(
        Guid id,
        [FromQuery] string reason = "",
        CancellationToken cancellationToken = default)
    {
        var command = new CancelAppointmentCommand { AppointmentId = id, Reason = reason };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Checks in to an appointment.
    /// </summary>
    [HttpPost("{id}/check-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CheckInAppointmentCommand { AppointmentId = id };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Completes an appointment.
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteAppointment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompleteAppointmentCommand { AppointmentId = id };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Gets appointments by type (Office, Telehealth, Phone).
    /// </summary>
    [HttpGet("by-type/{appointmentType}")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentsByType(
        string appointmentType,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? providerId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentsByTypeQuery
        {
            AppointmentType = appointmentType,
            PatientId = patientId,
            ProviderId = providerId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy" });
    }
}
