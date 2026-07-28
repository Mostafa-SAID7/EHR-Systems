namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Interface for push notification provider.
/// Implementations can use Firebase Cloud Messaging, Apple Push Notification, etc.
/// </summary>
public interface IPushNotificationProvider
{
    /// <summary>
    /// Send a push notification.
    /// </summary>
    /// <param name="deviceToken">Device token or user identifier.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body.</param>
    /// <param name="data">Additional data payload (optional).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message ID from provider if available.</returns>
    Task<string> SendPushAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to multiple devices.
    /// </summary>
    /// <param name="deviceTokens">List of device tokens.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body.</param>
    /// <param name="data">Additional data payload (optional).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Number of successfully sent notifications.</returns>
    Task<int> SendPushBatchAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if provider is configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync();
}
