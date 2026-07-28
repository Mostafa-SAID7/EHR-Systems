namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Logging-based push notification provider implementation.
/// Logs push notifications instead of sending them (useful for development/testing).
/// Can be replaced with Firebase Cloud Messaging, Apple Push Notification, etc.
/// </summary>
public class LoggingPushProvider : IPushNotificationProvider
{
    private readonly ILogger<LoggingPushProvider> _logger;

    public LoggingPushProvider(ILogger<LoggingPushProvider> logger)
    {
        _logger = logger;
    }

    public async Task<string> SendPushAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var dataJson = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : "{}";

        _logger.LogInformation(
            "PUSH (DEV/LOGGING): Device {DeviceToken} | Title: {Title} | Body: {Body} | Data: {Data}",
            deviceToken, title, body, dataJson);

        await Task.CompletedTask;
        return $"push-dev-{Guid.NewGuid()}";
    }

    public async Task<int> SendPushBatchAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();

        _logger.LogInformation(
            "PUSH BATCH (DEV/LOGGING): {DeviceCount} devices | Title: {Title} | Body: {Body}",
            tokens.Count, title, body);

        foreach (var token in tokens)
        {
            await SendPushAsync(token, title, body, data, cancellationToken);
        }

        return tokens.Count;
    }

    public async Task<bool> IsAvailableAsync()
    {
        await Task.CompletedTask;
        return true; // Always available in dev/logging mode
    }
}
