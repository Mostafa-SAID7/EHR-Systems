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
    /// Update invoice details (status, notes, payment info)
    /// </summary>
    [HttpPut("by-number/{invoiceNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInvoice(
        string invoiceNumber,
        [FromBody] UpdateInvoiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating invoice: {InvoiceNumber}", invoiceNumber);
        var command = new UpdateInvoiceCommand(invoiceNumber, request.Notes, request.Status, request.PaidAmount);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel an invoice with reason
    /// </summary>
    [HttpPost("by-number/{invoiceNumber}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvoice(
        string invoiceNumber,
        [FromBody] CancelInvoiceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling invoice: {InvoiceNumber}", invoiceNumber);
        var command = new CancelInvoiceCommand(invoiceNumber, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all invoices for a patient
    /// </summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientInvoices(
        Guid patientId,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving invoices for patient {PatientId}", patientId);
        var query = new GetPatientInvoicesQuery(patientId, status, fromDate, toDate, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generate PDF for an invoice
    /// </summary>
    [HttpPost("by-number/{invoiceNumber}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateInvoicePDF(
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF for invoice: {InvoiceNumber}", invoiceNumber);
        var command = new GenerateInvoicePDFCommand(invoiceNumber);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);
        
        return File(result.PdfContent, "application/pdf", result.FileName);
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
