namespace EHRPlatform.Services.Notification.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

/// <summary>
/// Notification API endpoints
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send notification via specified channel
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get user notifications (cached)
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await _mediator.Send(new GetUserNotificationsQuery
        {
            UserId = userId,
            PageNumber = page,
            PageSize = size
        });
        return Ok(result);
    }

    /// <summary>
    /// Set notification preferences
    /// </summary>
    [HttpPost("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPreferences([FromBody] SetNotificationPreferenceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPost("{notificationId}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(notificationId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete/archive a notification
    /// </summary>
    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(notificationId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get notification history for user with optional filters
    /// </summary>
    [HttpGet("user/{userId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationHistory(
        Guid userId,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotificationHistoryQuery(userId, type, fromDate, toDate, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all notification templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotificationTemplatesQuery(pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create notification template
    /// </summary>
    [HttpPost("templates")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateNotificationTemplateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new CreateNotificationTemplateCommand(request.TemplateName, request.Subject, request.Body, request.ContentType),
            cancellationToken);
        return result.Success ? CreatedAtAction(nameof(GetTemplates), new { id = result.TemplateId }, result) : BadRequest(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "NotificationService", timestamp = DateTime.UtcNow });
    }
}
