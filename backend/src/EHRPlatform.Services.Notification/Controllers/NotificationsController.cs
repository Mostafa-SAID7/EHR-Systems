using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Notification.Features.Notifications.Commands;
using EHRPlatform.Services.Notification.Features.Notifications.Queries;

namespace EHRPlatform.Services.Notification.Controllers;

/// <summary>
/// Notification endpoints.
/// Multi-channel delivery: Email, SMS, Push, In-App.
/// User preferences management.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send notification (Email, SMS, Push, or In-App).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetNotification), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get notification by ID (cached).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetNotificationQuery { NotificationId = id },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user notifications (cached, paginated).
    /// All sent/pending notifications for user.
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(NotificationListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetUserNotificationsQuery
            {
                UserId = userId,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Mark notification as sent (internal - called by notification processor).
    /// </summary>
    [HttpPost("{id}/mark-sent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationSent(
        Guid id,
        [FromBody] string? messageId = null,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new MarkNotificationSentCommand { NotificationId = id, MessageId = messageId },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Mark notification as failed (internal - called by notification processor).
    /// Triggers retries with exponential backoff.
    /// </summary>
    [HttpPost("{id}/mark-failed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationFailed(
        Guid id,
        [FromBody] string reason = "",
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new MarkNotificationFailedCommand { NotificationId = id, Reason = reason },
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get user notification preferences (cached).
    /// Shows which notification types are enabled per channel.
    /// </summary>
    [HttpGet("user/{userId}/preferences")]
    [ProducesResponseType(typeof(List<PreferenceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPreferences(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserPreferencesQuery { UserId = userId },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Set notification preference (opt-in/out for channel + type).
    /// </summary>
    [HttpPost("user/{userId}/preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPreference(
        Guid userId,
        [FromBody] SetNotificationPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { UserId = userId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Health check.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "notification-service" });
    }
}
