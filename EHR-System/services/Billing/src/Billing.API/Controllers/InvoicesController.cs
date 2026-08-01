using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Queries;
using EHRPlatform.Services.Billing.Contracts.Requests;
using EHRPlatform.Services.Billing.Contracts.Responses;

namespace EHRPlatform.Services.Billing.API.Controllers;

/// <summary>
/// Invoices API - Create, retrieve, and manage patient invoices.
/// HIPAA Compliant: All operations logged for audit purposes.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IMediator mediator, ILogger<InvoicesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger;
    }

    /// <summary>
    /// Get invoice by invoice number.
    /// </summary>
    [HttpGet("by-number/{invoiceNumber}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByNumber(
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving invoice: {InvoiceNumber}", invoiceNumber);
        var query = new GetInvoiceByNumberQuery(invoiceNumber);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound(new { message = $"Invoice {invoiceNumber} not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating invoice for patient {PatientId}", request.PatientId);

        var command = new CreateInvoiceCommand(
            request.PatientId,
            request.AppointmentId,
            request.ServiceDate,
            request.InsuranceProvider,
            request.InsurancePolicyNumber,
            request.Notes,
            request.LineItems);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetInvoiceByNumber), 
            new { invoiceNumber = result.InvoiceNumber }, result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "Billing API" });
    }
}
