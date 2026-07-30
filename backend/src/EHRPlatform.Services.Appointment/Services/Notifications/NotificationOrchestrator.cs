namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Notification orchestrator service.
/// Routes reminders to appropriate notification providers based on reminder type.
/// Handles email, SMS, and push notifications.
/// </summary>
public class NotificationOrchestrator
{
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;
    private readonly IPushNotificationProvider _pushProvider;
    private readonly ILogger<NotificationOrchestrator> _logger;

    public NotificationOrchestrator(
        IEmailProvider emailProvider,
        ISmsProvider smsProvider,
        IPushNotificationProvider pushProvider,
        ILogger<NotificationOrchestrator> logger)
    {
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
        _pushProvider = pushProvider;
        _logger = logger;
    }

    /// <summary>
    /// Send appointment reminder notification.
    /// </summary>
    public async Task<string> SendReminderAsync(
        string recipientIdentifier,
        string reminderType,
        string templateName,
        Dictionary<string, string> templateVars,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending {ReminderType} reminder to {Recipient} using template {Template}",
            reminderType, recipientIdentifier, templateName);

        try
        {
            templateVars.TryGetValue("AppointmentDate", out var appointmentDate);
            var pushTitle = "Appointment: " + (appointmentDate ?? "Upcoming");

            var messageId = reminderType.ToLower() switch
            {
                "email" => await _emailProvider.SendEmailFromTemplateAsync(
                    recipientIdentifier, templateName, templateVars, cancellationToken),

                "sms" => await _smsProvider.SendSmsFromTemplateAsync(
                    recipientIdentifier, templateName, templateVars, cancellationToken),

                "push" => await _pushProvider.SendPushAsync(
                    recipientIdentifier,
                    pushTitle,
                    "You have an appointment reminder",
                    templateVars,
                    cancellationToken),

                "inapp" => await SendInAppAsync(recipientIdentifier, templateName, templateVars),

                _ => throw new InvalidOperationException($"Unknown reminder type: {reminderType}")
            };

            _logger.LogInformation(
                "Reminder sent successfully. Type: {ReminderType}, MessageId: {MessageId}",
                reminderType, messageId);

            return messageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send {ReminderType} reminder to {Recipient}",
                reminderType, recipientIdentifier);

            throw;
        }
    }

    /// <summary>
    /// Check which notification providers are available.
    /// </summary>
    public async Task<NotificationProviderStatus> GetProviderStatusAsync()
    {
        return new NotificationProviderStatus
        {
            EmailAvailable = await _emailProvider.IsAvailableAsync(),
            SmsAvailable = await _smsProvider.IsAvailableAsync(),
            PushAvailable = await _pushProvider.IsAvailableAsync(),
            InAppAvailable = true // Always available
        };
    }

    private async Task<string> SendInAppAsync(
        string userId,
        string templateName,
        Dictionary<string, string> templateVars)
    {
        // In-app notifications would be stored in database
        // and delivered via SignalR or polling in real application
        var appointmentDate = templateVars.TryGetValue("AppointmentDate", out var ad) ? ad : "upcoming";
        var message = $"Appointment {templateName} on {appointmentDate}";

        _logger.LogInformation("In-app notification for {UserId}: {Message}", userId, message);

        await Task.CompletedTask;
        return $"inapp-{Guid.NewGuid()}";
    }
}

/// <summary>
/// Status of notification providers.
/// </summary>
public class NotificationProviderStatus
{
    public bool EmailAvailable { get; set; }
    public bool SmsAvailable { get; set; }
    public bool PushAvailable { get; set; }
    public bool InAppAvailable { get; set; }

    public int AvailableProviders => 
        (EmailAvailable ? 1 : 0) + 
        (SmsAvailable ? 1 : 0) + 
        (PushAvailable ? 1 : 0) + 
        (InAppAvailable ? 1 : 0);
}
