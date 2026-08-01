namespace EHRPlatform.Services.Integration.API.Controllers;

using MediatR;
using EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;
using EHRPlatform.Services.Integration.Application.Features.NPHIES.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// NPHIES Insurance Claims API - Insurance claim submission and tracking
/// Single Responsibility: NPHIES operations only
/// For HL7 message processing, see HL7Controller
/// </summary>
[ApiController]
[Route("api/v1/integration/nphies")]
[Authorize]
public class NPHIESController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NPHIESController> _logger;

    public NPHIESController(IMediator mediator, ILogger<NPHIESController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Submit claim to NPHIES (Saudi healthcare insurance)
    /// </summary>
    [HttpPost("claims/submit")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitNPHIESClaim(
        [FromBody] SubmitNPHIESClaimRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Submitting NPHIES claim for patient {PatientId}, provider {ProviderId}",
            request.PatientId, request.ProviderId);

        var command = new SubmitNPHIESClaimCommand
        {
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            ClaimAmount = request.ClaimAmount,
            ServiceDate = request.ServiceDate,
            Diagnosis = request.Diagnosis,
            Procedures = request.Procedures
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetClaimStatus), new { claimId = result.ClaimId }, result);
    }

    /// <summary>
    /// Get NPHIES claim status
    /// </summary>
    [HttpGet("claims/{claimId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimStatus(
        [FromRoute] Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting NPHIES claim status for claim {ClaimId}", claimId);

        var query = new GetNPHIESClaimStatusQuery { ClaimId = claimId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retry NPHIES claim submission
    /// </summary>
    [HttpPost("claims/{claimId:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryNPHIESClaim(
        [FromRoute] Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying NPHIES claim {ClaimId}", claimId);

        var command = new RetryNPHIESSubmissionCommand { ClaimId = claimId };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "NPHIESService", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Submit NPHIES claim request
/// </summary>
public class SubmitNPHIESClaimRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public decimal ClaimAmount { get; set; }
    public DateTime ServiceDate { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public List<string> Procedures { get; set; } = new();
}
