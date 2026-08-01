namespace EHRPlatform.Services.Notification.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;
using EHRPlatform.Services.Notification.Contracts.Requests;

/// <summary>
/// Notification Templates API - Manage notification templates
/// Single Responsibility: Notification template management only
/// Separated from notification operations (configuration vs operational data)
/// </summary>
[ApiController]
[Route("api/v1/notification-templates")]
[Authorize]
public class NotificationTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationTemplatesController> _logger;

    public NotificationTemplatesController(IMediator mediator, ILogger<NotificationTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all notification templates (paginated)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting notification templates, page {PageNumber}", pageNumber);
        var result = await _mediator.Send(
            new GetNotificationTemplatesQuery(pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("{templateId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting notification template {TemplateId}", templateId);
        // Query implementation would go here
        return Ok(new { templateId });
    }

    /// <summary>
    /// Create notification template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateNotificationTemplateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating notification template: {TemplateName}", request.TemplateName);
        
        if (string.IsNullOrEmpty(request.TemplateName))
            return BadRequest("Template name is required");

        var result = await _mediator.Send(
            new CreateNotificationTemplateCommand(request.TemplateName, request.Subject, request.Body, request.ContentType),
            cancellationToken);
        
        return result.Success 
            ? CreatedAtAction(nameof(GetTemplate), new { templateId = result.TemplateId }, result) 
            : BadRequest(result);
    }

    /// <summary>
    /// Update notification template
    /// </summary>
    [HttpPut("{templateId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTemplate(
        Guid templateId,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating notification template {TemplateId}", templateId);
        
        if (templateId == Guid.Empty)
            return BadRequest("TemplateId cannot be empty");

        // Update implementation would go here
        return NoContent();
    }

    /// <summary>
    /// Delete notification template
    /// </summary>
    [HttpDelete("{templateId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting notification template {TemplateId}", templateId);
        
        if (templateId == Guid.Empty)
            return BadRequest("TemplateId cannot be empty");

        // Delete implementation would go here
        return NoContent();
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "NotificationTemplatesService", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Update notification template request DTO
/// </summary>
public class UpdateNotificationTemplateRequest
{
    public string? TemplateName { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
}
