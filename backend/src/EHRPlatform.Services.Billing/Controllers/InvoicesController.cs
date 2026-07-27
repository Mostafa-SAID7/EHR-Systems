using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Billing.Application.Invoicing.Responses;
using EHRPlatform.Services.Billing.Features.Invoicing.Commands;
using EHRPlatform.Services.Billing.Features.Invoicing.Queries;
using EHRPlatform.Services.Billing.Features.Claims.Commands;
using EHRPlatform.Services.Billing.Features.Payments.Commands;

namespace EHRPlatform.Services.Billing.Controllers;

/// <summary>
/// Manages invoice creation, payments, and insurance submission.
/// Entities: Invoice aggregate with LineItems and Payments.
/// </summary>
[ApiController]
[Route("api/v1/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IMediator mediator,
        ILogger<InvoicesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets invoice by invoice number (slug-based URL).
    /// </summary>
    [HttpGet("by-number/{invoiceNumber}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByNumber(
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvoiceByNumberQuery { InvoiceNumber = invoiceNumber };
        var result = await _mediator.Send(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetInvoiceByNumber), new { invoiceNumber = result.InvoiceNumber }, result);
    }

    /// <summary>
    /// Records a payment for an invoice.
    /// </summary>
    [HttpPost("{invoiceId}/payments")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(
        Guid invoiceId,
        [FromBody] RecordPaymentCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        var command = baseCommand with { InvoiceId = invoiceId };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Submits invoice to insurance.
    /// </summary>
    [HttpPost("{invoiceId}/submit-insurance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitToInsurance(
        Guid invoiceId,
        [FromBody] SubmitToInsuranceCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        var command = baseCommand with { InvoiceId = invoiceId };
        await _mediator.Send(command, cancellationToken);
        return Ok();
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
