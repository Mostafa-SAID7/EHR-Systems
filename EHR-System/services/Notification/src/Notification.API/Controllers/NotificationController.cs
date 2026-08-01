namespace EHRPlatform.Services.Notification.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

/// <summary>
/// Notifications API - Send and manage notifications
/// Single Responsibility: Notification operations only
/// For template management, see NotificationTemplatesController
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Send notification via specified channel
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending notification to user {UserId} via {Channel}", command.UserId, command.Channel);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get user notifications (cached)
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting notifications for user {UserId}, page {Page}", userId, page);
        var result = await _mediator.Send(new GetUserNotificationsQuery
        {
            UserId = userId,
            PageNumber = page,
            PageSize = size
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Set notification preferences
    /// </summary>
    [HttpPost("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPreferences(
        [FromBody] SetNotificationPreferenceCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting notification preferences for user {UserId}", command.UserId);
        var result = await _mediator.Send(command, cancellationToken);
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
        _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);
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
        _logger.LogInformation("Deleting notification {NotificationId}", notificationId);
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
        _logger.LogInformation(
            "Getting notification history for user {UserId}, type={Type}, from={FromDate} to={ToDate}",
            userId, type, fromDate, toDate);
        var result = await _mediator.Send(
            new GetNotificationHistoryQuery(userId, type, fromDate, toDate, pageNumber, pageSize),
            cancellationToken);
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
        return Ok(new { status = "healthy", service = "NotificationService", timestamp = DateTime.UtcNow });
    }
}
    {
        return Ok(new { status = "healthy", service = "NotificationService", timestamp = DateTime.UtcNow });
    }
}
