using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Data.Models;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Controllers;

/// <summary>
/// Tag management for appointments.
/// Separated concern: Tags operate on Appointment entities.
/// Route: GET/POST/PUT/DELETE /api/v1/appointments/{appointmentId}/tags
/// </summary>
[ApiController]
[Route("api/v1/appointments/{appointmentId}/tags")]
public class AppointmentTagsController : ControllerBase
{
    private readonly ITagQueryService _tagQueryService;
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentTagsController> _logger;

    public AppointmentTagsController(
        ITagQueryService tagQueryService,
        IMediator mediator,
        ILogger<AppointmentTagsController> logger)
    {
        _tagQueryService = tagQueryService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all tags for an appointment.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentTags(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await _tagQueryService.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                cancellationToken);
            return Ok(new { appointmentId, tags });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Applies tags to an appointment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyAppointmentTags(
        Guid appointmentId,
        [FromBody] ApplyTagsCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = baseCommand with
            {
                ResourceType = nameof(Appointment),
                ResourceId = appointmentId,
                ServiceName = "Appointment"
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying tags to appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Removes a tag from an appointment.
    /// </summary>
    [HttpDelete("{tagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAppointmentTag(
        Guid appointmentId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveTagCommand
            {
                ResourceType = nameof(Appointment),
                ResourceId = appointmentId,
                TagId = tagId,
                ServiceName = "Appointment"
            };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag from appointment");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Replaces all tags for an appointment.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAppointmentTags(
        Guid appointmentId,
        [FromBody] SetResourceTagsCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = baseCommand with
            {
                ResourceType = nameof(Appointment),
                ResourceId = appointmentId,
                ServiceName = "Appointment"
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tags for appointment");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
