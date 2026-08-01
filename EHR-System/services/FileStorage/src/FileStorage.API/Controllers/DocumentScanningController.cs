namespace EHRPlatform.Services.FileStorage.API.Controllers;

using MediatR;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// API endpoints for document virus scanning operations.
/// </summary>
[ApiController]
[Route("api/v1/documents")]
[Authorize]
public class DocumentScanningController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentScanningController> _logger;

    public DocumentScanningController(IMediator mediator, ILogger<DocumentScanningController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates virus scanning for a document.
    /// POST /api/v1/documents/{documentId}/scan
    /// </summary>
    [HttpPost("{documentId:guid}/scan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScanDocument(
        [FromRoute] Guid documentId,
        [FromBody] ScanDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning document {DocumentId}", documentId);

        var command = new ScanDocumentCommand
        {
            DocumentId = documentId,
            S3Key = request.S3Key
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves virus scan result for a document.
    /// GET /api/v1/documents/{documentId}/scan-result
    /// </summary>
    [HttpGet("{documentId:guid}/scan-result")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScanResult(
        [FromRoute] Guid documentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting scan result for document {DocumentId}", documentId);

        var query = new GetVirusScanResultQuery { DocumentId = documentId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a document (scheduled or immediate).
    /// DELETE /api/v1/documents/{documentId}
    /// </summary>
    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        [FromRoute] Guid documentId,
        [FromBody] DeleteDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting document {DocumentId}. Immediate: {IsImmediate}", 
            documentId, request.IsImmediate);

        var command = new DeleteDocumentCommand
        {
            DocumentId = documentId,
            Reason = request.Reason,
            IsImmediate = request.IsImmediate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets retention status for a document.
    /// GET /api/v1/documents/{documentId}/retention-status
    /// </summary>
    [HttpGet("{documentId:guid}/retention-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRetentionStatus(
        [FromRoute] Guid documentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting retention status for document {DocumentId}", documentId);

        var query = new GetDocumentRetentionStatusQuery { DocumentId = documentId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates retention policy for a document.
    /// PUT /api/v1/documents/{documentId}/retention-policy
    /// </summary>
    [HttpPut("{documentId:guid}/retention-policy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRetentionPolicy(
        [FromRoute] Guid documentId,
        [FromBody] UpdateRetentionPolicyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating retention policy for document {DocumentId}", documentId);

        var command = new UpdateRetentionPolicyCommand
        {
            DocumentId = documentId,
            RetentionPolicyId = request.RetentionPolicyId,
            RetentionDays = request.RetentionDays,
            CustomExpirationDate = request.CustomExpirationDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// Request DTOs
public class ScanDocumentRequest
{
    [Required]
    public string S3Key { get; set; } = string.Empty;
}

public class DeleteDocumentRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    public bool IsImmediate { get; set; } = false;
}

public class UpdateRetentionPolicyRequest
{
    [Required]
    public Guid RetentionPolicyId { get; set; }
    [Required]
    public int RetentionDays { get; set; }
    public DateTime? CustomExpirationDate { get; set; }
}
