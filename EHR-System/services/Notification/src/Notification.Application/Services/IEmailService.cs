namespace EHRPlatform.Services.Notification.Application.Services;

/// <summary>
/// Service for sending emails via SendGrid or AWS SES.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email to user.
    /// Returns MessageId from provider.
    /// </summary>
    Task<string> SendEmailAsync(Guid userId, string subject, string body, CancellationToken cancellationToken = default);
}

public interface ISmsService
{
    /// <summary>
    /// Send SMS to user via Twilio.
    /// </summary>
    Task<string> SendSmsAsync(Guid userId, string message, CancellationToken cancellationToken = default);
}

public interface IPushService
{
    /// <summary>
    /// Send push notification via Firebase Cloud Messaging.
    /// </summary>
    Task<string> SendPushAsync(Guid userId, string title, string body, CancellationToken cancellationToken = default);
}
