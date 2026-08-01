using MediatR;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;
using EHRPlatform.Services.FileStorage.Contracts.Responses;

namespace EHRPlatform.Services.FileStorage.API.Controllers;

/// <summary>
/// Documents API - Upload, retrieve, and manage patient documents.
/// HIPAA Compliant: All document access is logged for audit purposes.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger;
    }

    /// <summary>
    /// Get document by ID.
    /// </summary>
    [HttpGet("{documentId}")]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving document: {DocumentId}", documentId);
        var query = new GetDocumentQuery(documentId);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound(new { message = $"Document {documentId} not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "FileStorage API" });
    }
}
