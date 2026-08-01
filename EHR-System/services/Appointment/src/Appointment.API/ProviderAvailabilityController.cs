using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Commands;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;
using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Queries;

namespace EHRPlatform.Services.Appointment.Controllers;

/// <summary>
/// Manages provider availability scheduling and calendar queries.
/// Separate concern: Provider-focused availability vs. Patient-focused appointment lifecycle.
/// </summary>
[ApiController]
[Route("api/v1/providers")]
public class ProviderAvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProviderAvailabilityController> _logger;

    public ProviderAvailabilityController(
        IMediator mediator,
        ILogger<ProviderAvailabilityController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets provider calendar (all appointments for a specific date).
    /// </summary>
    [HttpGet("{providerId}/calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderCalendar(
        Guid providerId,
        [FromQuery] DateTime? date = null,
        [FromQuery] string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var calendarDate = date ?? DateTime.UtcNow.Date;
        var query = new GetProviderAppointmentsQuery
        {
            ProviderId = providerId,
            CalendarDate = calendarDate,
            StatusFilter = statusFilter
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets provider availability slots for a date range.
    /// </summary>
    [HttpGet("{providerId}/availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderAvailability(
        Guid providerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProviderAvailabilityQuery
        {
            ProviderId = providerId,
            FromDate = fromDate ?? DateTime.UtcNow,
            ToDate = toDate ?? DateTime.UtcNow.AddDays(30)
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sets provider availability slots.
    /// </summary>
    [HttpPost("{providerId}/availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAvailability(
        Guid providerId,
        [FromBody] SetProviderAvailabilityCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        var command = baseCommand with { ProviderId = providerId };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
