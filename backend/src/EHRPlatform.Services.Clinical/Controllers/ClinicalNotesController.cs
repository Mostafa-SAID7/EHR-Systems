using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;

namespace EHRPlatform.Services.Clinical.Controllers;

/// <summary>
/// Clinical notes endpoints.
/// SOAP format notes, vitals, diagnoses, procedures, timeline views.
/// </summary>
[ApiController]
[Route("api/v1/clinical")]
[Authorize]
public class ClinicalNotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicalNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create clinical note (SOAP format, draft status).
    /// </summary>
    [HttpPost("notes")]
    [ProducesResponseType(typeof(ClinicalNoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNote(
        [FromBody] CreateClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetNote), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get clinical note by ID (cached).
    /// Includes vitals, diagnoses, procedures.
    /// </summary>
    [HttpGet("notes/{id}")]
    [ProducesResponseType(typeof(ClinicalNoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNote(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClinicalNoteQuery { ClinicalNoteId = id },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update SOAP components (Subjective, Objective, Assessment, Plan).
    /// Only available on draft notes.
    /// </summary>
    [HttpPut("notes/{id}/soap")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSOAP(
        Guid id,
        [FromBody] UpdateSOAPCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { ClinicalNoteId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Record vital signs for note.
    /// Publishes VitalSignsRecordedEvent.
    /// </summary>
    [HttpPost("notes/{id}/vitals")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordVitals(
        Guid id,
        [FromBody] RecordVitalsCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { ClinicalNoteId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Add diagnosis to note (ICD-10 code).
    /// Publishes DiagnosisRecordedEvent.
    /// </summary>
    [HttpPost("notes/{id}/diagnoses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDiagnosis(
        Guid id,
        [FromBody] AddDiagnosisCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { ClinicalNoteId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Add procedure to note (CPT/SNOMED code).
    /// Publishes ProcedurePerformedEvent.
    /// </summary>
    [HttpPost("notes/{id}/procedures")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProcedure(
        Guid id,
        [FromBody] AddProcedureCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { ClinicalNoteId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Finalize clinical note (lock for editing).
    /// Publishes ClinicalNoteCompletedEvent.
    /// </summary>
    [HttpPost("notes/{id}/finalize")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinializeNote(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new FinalizeClinicalNoteCommand { ClinicalNoteId = id },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get patient clinical timeline (all notes, paginated, cached).
    /// </summary>
    [HttpGet("patients/{patientId}/timeline")]
    [ProducesResponseType(typeof(ClinicalNoteListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicalTimeline(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPatientClinicalTimelineQuery 
            { 
                PatientId = patientId, 
                PageNumber = page, 
                PageSize = pageSize 
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get patient vital signs timeline (cached).
    /// Optional date range filtering.
    /// </summary>
    [HttpGet("patients/{patientId}/vitals/timeline")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVitalsTimeline(
        Guid patientId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetVitalSignsTimelineQuery 
            { 
                PatientId = patientId, 
                FromDate = from, 
                ToDate = to 
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get patient diagnosis history (cached).
    /// All ICD-10 diagnoses with dates.
    /// </summary>
    [HttpGet("patients/{patientId}/diagnoses/history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDiagnosisHistory(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetDiagnosisHistoryQuery { PatientId = patientId },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Export clinical note/encounter as FHIR R4 Bundle JSON.
    /// Interoperability endpoint for HL7 FHIR integration.
    /// </summary>
    [HttpGet("{id}/fhir")]
    [Produces("application/fhir+json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportFhir(
        Guid id,
        CancellationToken cancellationToken)
    {
        var fhirJson = await _mediator.Send(
            new Features.ClinicalNotes.Queries.ExportFhirEncounterQuery(id),
            cancellationToken);

        return Content(fhirJson, "application/fhir+json");
    }

    /// <summary>
    /// Health check.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "clinical-service" });
    }
}

