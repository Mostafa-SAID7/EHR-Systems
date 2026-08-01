namespace EHRPlatform.Services.Integration.API.Controllers;

using MediatR;
using EHRPlatform.Services.Integration.Application.Features.HL7.Commands;
using EHRPlatform.Services.Integration.Application.Features.HL7.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// HL7 Message API - HL7 message processing and transformation
/// Single Responsibility: HL7 operations only
/// For NPHIES insurance claims, see NPHIESController
/// </summary>
[ApiController]
[Route("api/v1/integration/hl7")]
[Authorize]
public class HL7Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HL7Controller> _logger;

    public HL7Controller(IMediator mediator, ILogger<HL7Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Receive and parse HL7 message
    /// </summary>
    [HttpPost("receive")]
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
    /// Get HL7 message processing status
    /// </summary>
    [HttpGet("{messageId:guid}/status")]
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
    /// Transform HL7 message to FHIR format
    /// </summary>
    [HttpPost("{messageId:guid}/transform-fhir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransformToFHIR(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transforming HL7 message {MessageId} to FHIR", messageId);

        var command = new TransformToFHIRCommand { MessageId = messageId };
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
        return Ok(new { status = "healthy", service = "HL7Service", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// HL7 message receive request
/// </summary>
public class ReceiveHL7MessageRequest
{
    public string HL7Content { get; set; } = string.Empty;
    public string SendingApplication { get; set; } = string.Empty;
    public string ReceivingApplication { get; set; } = string.Empty;
}

/// <summary>
/// Transform to FHIR command
/// </summary>
public class TransformToFHIRCommand : IRequest<TransformToFHIRResponse>
{
    public Guid MessageId { get; set; }
}

/// <summary>
/// Transform to FHIR response
/// </summary>
public class TransformToFHIRResponse
{
    public Guid FhirBundleId { get; set; }
    public string FhirContent { get; set; } = string.Empty;
}
