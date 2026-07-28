namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Interface for SMS notification provider.
/// Implementations can use Twilio, AWS SNS, etc.
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Send an SMS notification.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (E.164 format: +1234567890).</param>
    /// <param name="message">SMS message body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message ID from provider if available.</returns>
    Task<string> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send SMS with template.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (E.164 format).</param>
    /// <param name="templateName">Name of the SMS template.</param>
    /// <param name="templateVars">Variables to populate the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message ID from provider.</returns>
    Task<string> SendSmsFromTemplateAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> templateVars,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if provider is configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync();
}
