namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Interface for email notification provider.
/// Implementations can use AWS SES, SendGrid, SMTP, etc.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Send an email notification.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (HTML supported).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message ID from provider if available.</returns>
    Task<string> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email with template.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="templateName">Name of the email template.</param>
    /// <param name="templateVars">Variables to populate the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message ID from provider.</returns>
    Task<string> SendEmailFromTemplateAsync(
        string to,
        string templateName,
        Dictionary<string, string> templateVars,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if provider is configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync();
}
