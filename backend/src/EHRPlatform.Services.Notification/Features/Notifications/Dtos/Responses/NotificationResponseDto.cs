namespace EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;

/// <summary>
/// Notification response DTO.
/// Single Responsibility: Represent notification in API responses.
/// </summary>
public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? MessageId { get; set; }
    public string? Recipient { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
