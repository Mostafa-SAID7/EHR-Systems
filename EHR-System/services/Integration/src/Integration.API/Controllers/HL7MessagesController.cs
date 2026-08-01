namespace EHRPlatform.Services.Integration.API.Controllers;

using MediatR;
using EHRPlatform.Services.Integration.Application.Features.HL7.Commands;
using EHRPlatform.Services.Integration.Application.Features.HL7.Queries;
using EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;
using EHRPlatform.Services.Integration.Application.Features.NPHIES.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// API endpoints for HL7 message and NPHIES claim operations.
/// </summary>
[ApiController]
[Route("api/v1/integration")]
[Authorize]
public class HL7MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HL7MessagesController> _logger;

    public HL7MessagesController(IMediator mediator, ILogger<HL7MessagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Receive and parse HL7 message.
    /// POST /api/v1/integration/hl7/receive
    /// </summary>
    [HttpPost("hl7/receive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveHL7Message(
        [FromBody] ReceiveHL7MessageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receiving HL7 message from {SendingApp}", request.SendingApplication);

        var command = new ReceiveHL7MessageCommand
        {
            HL7Content = request.HL7Content,
            SendingApplication = request.SendingApplication,
            ReceivingApplication = request.ReceivingApplication
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get HL7 message processing status.
    /// GET /api/v1/integration/hl7/{messageId}/status
    /// </summary>
    [HttpGet("hl7/{messageId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHL7Status(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting HL7 status for message {MessageId}", messageId);

        var query = new GetHL7MessageStatusQuery { MessageId = messageId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Transform HL7 message to FHIR format.
    /// POST /api/v1/integration/hl7/{messageId}/transform-fhir
    /// </summary>
    [HttpPost("hl7/{messageId:guid}/transform-fhir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransformToFHIR(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transforming HL7 {MessageId} to FHIR", messageId);

        var command = new TransformToFHIRCommand { HL7MessageId = messageId };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submit claim to NPHIES.
    /// POST /api/v1/integration/nphies/claims/submit
    /// </summary>
    [HttpPost("nphies/claims/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitNPHIESClaim(
        [FromBody] SubmitNPHIESClaimRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting NPHIES claim for message {MessageId}", request.HL7MessageId);

        var command = new SubmitNPHIESClaimCommand
        {
            HL7MessageId = request.HL7MessageId,
            FHIRTransformationId = request.FHIRTransformationId,
            ClaimType = request.ClaimType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get NPHIES claim status.
    /// GET /api/v1/integration/nphies/claims/{claimId}/status
    /// </summary>
    [HttpGet("nphies/claims/{claimId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimStatus(
        [FromRoute] Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting claim status for {ClaimId}", claimId);

        var query = new GetClaimStatusQuery { ClaimId = claimId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retry failed NPHIES claim submission.
    /// POST /api/v1/integration/nphies/claims/{claimId}/retry
    /// </summary>
    [HttpPost("nphies/claims/{claimId:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryNPHIESClaim(
        [FromRoute] Guid claimId,
        [FromBody] RetryNPHIESClaimRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying NPHIES claim {ClaimId}", claimId);

        var command = new RetryNPHIESSubmissionCommand
        {
            ClaimId = claimId,
            MaxRetries = request.MaxRetries
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// Request DTOs
public class ReceiveHL7MessageRequest
{
    [Required]
    public string HL7Content { get; set; } = string.Empty;
    public string? SendingApplication { get; set; }
    public string? ReceivingApplication { get; set; }
}

public class SubmitNPHIESClaimRequest
{
    [Required]
    public Guid HL7MessageId { get; set; }
    public Guid? FHIRTransformationId { get; set; }
    [Required]
    public string ClaimType { get; set; } = string.Empty;
}

public class RetryNPHIESClaimRequest
{
    public int MaxRetries { get; set; } = 3;
}
