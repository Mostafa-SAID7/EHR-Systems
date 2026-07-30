using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

namespace EHRPlatform.Services.Prescription.Controllers;

/// <summary>
/// Prescription management endpoints.
/// Issue, refill, suspend, discontinue medications.
/// </summary>
[ApiController]
[Route("api/v1/prescriptions")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrescriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Issue new prescription.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IssuePrescription(
        [FromBody] IssuePrescriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPrescription), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get prescription by ID (cached).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PrescriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrescription(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetPrescriptionQuery { PrescriptionId = id },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get patient active prescriptions (cached, paginated).
    /// </summary>
    [HttpGet("patient/{patientId}/active")]
    [ProducesResponseType(typeof(PagedResult<PrescriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePrescriptions(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPatientActivePrescriptionsQuery
            {
                PatientId = patientId,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get patient prescription history (cached, paginated).
    /// </summary>
    [HttpGet("patient/{patientId}/history")]
    [ProducesResponseType(typeof(PagedResult<PrescriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrescriptionHistory(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPatientPrescriptionHistoryQuery
            {
                PatientId = patientId,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Request refill (patient-initiated or patient portal).
    /// </summary>
    [HttpPost("{id}/refill-request")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestRefill(
        Guid id,
        [FromBody] string? pharmacyId = null,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new RequestRefillCommand { PrescriptionId = id, PharmacyId = pharmacyId },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Approve refill request (provider action).
    /// </summary>
    [HttpPost("{id}/refill/{refillId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRefill(
        Guid id,
        Guid refillId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ApproveRefillCommand { PrescriptionId = id, RefillId = refillId },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get pending refill requests for provider (cached).
    /// </summary>
    [HttpGet("provider/{providerId}/pending-refills")]
    [ProducesResponseType(typeof(RefillRequestListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingRefills(
        Guid providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPendingRefillsQuery
            {
                ProviderId = providerId,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Suspend prescription (e.g., for drug interactions).
    /// </summary>
    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendPrescription(
        Guid id,
        [FromBody] string reason = "",
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new SuspendPrescriptionCommand { PrescriptionId = id, Reason = reason },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Resume suspended prescription.
    /// </summary>
    [HttpPost("{id}/resume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumePrescription(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResumePrescriptionCommand { PrescriptionId = id },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Discontinue prescription.
    /// </summary>
    [HttpPost("{id}/discontinue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DiscontinuePrescription(
        Guid id,
        [FromBody] string reason = "",
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new DiscontinuePrescriptionCommand { PrescriptionId = id, Reason = reason },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Health check.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "prescription-service" });
    }
}

