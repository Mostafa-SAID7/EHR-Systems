namespace EHRPlatform.Services.Appointment.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;
using EHRPlatform.Services.Appointment.Controllers.Requests;

/// <summary>
/// Appointment Notes API - Add and manage appointment notes
/// Single Responsibility: Appointment note management only
/// Separated from core appointments for independent scaling
/// </summary>
[ApiController]
[Route("api/v1/appointments/{appointmentId}/notes")]
[Authorize]
public class AppointmentNotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentNotesController> _logger;

    public AppointmentNotesController(
        IMediator mediator,
        ILogger<AppointmentNotesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Add a note to an appointment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddNote(
        Guid appointmentId,
        [FromBody] AddNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding note to appointment {AppointmentId} by user {CreatedById}",
            appointmentId, request.CreatedById);

        try
        {
            if (appointmentId == Guid.Empty)
                return BadRequest("AppointmentId cannot be empty");

            if (string.IsNullOrEmpty(request.Content))
                return BadRequest("Note content cannot be empty");

            var command = new AddNoteCommand
            {
                AppointmentId = appointmentId,
                Content = request.Content,
                CreatedById = request.CreatedById,
                PrivacyLevel = request.PrivacyLevel,
                Category = request.Category
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Note added successfully to appointment {AppointmentId}", appointmentId);

            return CreatedAtAction(nameof(AddNote), new { appointmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to appointment {AppointmentId}", appointmentId);
            return StatusCode(500, "Error adding note");
        }
    }

    /// <summary>
    /// Get notes for an appointment
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotes(
        Guid appointmentId,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting notes for appointment {AppointmentId}, page {Page}",
            appointmentId, page);

        try
        {
            if (appointmentId == Guid.Empty)
                return BadRequest("AppointmentId cannot be empty");

            // Query implementation would go here
            // For now, returning placeholder
            return Ok(new { appointmentId, notes = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, "Error retrieving notes");
        }
    }

    /// <summary>
    /// Delete a note from an appointment
    /// </summary>
    [HttpDelete("{noteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNote(
        Guid appointmentId,
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting note {NoteId} from appointment {AppointmentId}",
            noteId, appointmentId);

        try
        {
            // Delete implementation would go here
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId}", noteId);
            return StatusCode(500, "Error deleting note");
        }
    }
}
