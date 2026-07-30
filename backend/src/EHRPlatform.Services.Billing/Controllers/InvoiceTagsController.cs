using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Data.Models;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Billing.Domain.Entities;

namespace EHRPlatform.Services.Billing.Controllers;

/// <summary>
/// Tag management for invoices.
/// Separated concern: Tags operate on Invoice entities.
/// Route: GET/POST/PUT/DELETE /api/v1/invoices/{invoiceId}/tags
/// </summary>
[ApiController]
[Route("api/v1/invoices/{invoiceId}/tags")]
public class InvoiceTagsController : ControllerBase
{
    private readonly ITagQueryService _tagQueryService;
    private readonly IMediator _mediator;
    private readonly ILogger<InvoiceTagsController> _logger;

    public InvoiceTagsController(
        ITagQueryService tagQueryService,
        IMediator mediator,
        ILogger<InvoiceTagsController> logger)
    {
        _tagQueryService = tagQueryService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all tags for an invoice.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceTags(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await _tagQueryService.GetResourceTagsAsync(
                invoiceId,
                nameof(Invoice),
                cancellationToken);
            return Ok(new { invoiceId, tags });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for invoice {InvoiceId}", invoiceId);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Applies tags to an invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyInvoiceTags(
        Guid invoiceId,
        [FromBody] ApplyTagsCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = baseCommand with
            {
                ResourceType = nameof(Invoice),
                ResourceId = invoiceId,
                ServiceName = "Billing"
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying tags to invoice {InvoiceId}", invoiceId);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Removes a tag from an invoice.
    /// </summary>
    [HttpDelete("{tagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveInvoiceTag(
        Guid invoiceId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveTagCommand
            {
                ResourceType = nameof(Invoice),
                ResourceId = invoiceId,
                TagId = tagId,
                ServiceName = "Billing"
            };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag from invoice");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Replaces all tags for an invoice.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetInvoiceTags(
        Guid invoiceId,
        [FromBody] SetResourceTagsCommand baseCommand,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = baseCommand with
            {
                ResourceType = nameof(Invoice),
                ResourceId = invoiceId,
                ServiceName = "Billing"
            };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tags for invoice");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
