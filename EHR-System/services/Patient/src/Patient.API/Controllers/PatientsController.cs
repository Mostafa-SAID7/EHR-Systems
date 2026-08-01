namespace EHRPlatform.Services.Patient.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;
using EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

/// <summary>
/// Patients API endpoints
/// </summary>
[ApiController]
[Route("api/v1/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create new patient
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get patient by ID (cached 10 minutes)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient(Guid id)
    {
        var result = await _mediator.Send(new GetPatientQuery { PatientId = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Update patient information
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientCommand command)
    {
        command.PatientId = id;
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Full-text search patients (Elasticsearch)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPatients([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await _mediator.Send(new SearchPatientsQuery
        {
            SearchTerm = q,
            PageNumber = page,
            PageSize = size
        });
        return Ok(result);
    }

    /// <summary>
    /// Add allergy to patient record
    /// </summary>
    [HttpPost("{id}/allergies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAllergy(Guid id, [FromBody] AddAllergyCommand command)
    {
        command.PatientId = id;
        var result = await _mediator.Send(command);
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
        return Ok(new { status = "healthy", service = "PatientService", timestamp = DateTime.UtcNow });
    }
}
