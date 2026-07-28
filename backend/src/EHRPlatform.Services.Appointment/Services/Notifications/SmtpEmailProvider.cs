using System.Net;
using System.Net.Mail;

namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// SMTP-based email provider implementation.
/// Sends emails via SMTP server (Gmail, Office 365, custom SMTP, etc.)
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public SmtpEmailProvider(IConfiguration configuration, ILogger<SmtpEmailProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
            var smtpUser = _configuration["Email:Smtp:User"];
            var smtpPassword = _configuration["Email:Smtp:Password"];
            var fromEmail = _configuration["Email:FromAddress"] ?? smtpUser;

            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogWarning("SMTP provider not configured");
                return "UNCONFIGURED";
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                client.Timeout = 10000;

                var mailMessage = new MailMessage(fromEmail, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage, cancellationToken);

                _logger.LogInformation("Email sent successfully to {To}", to);
                return $"smtp-{Guid.NewGuid()}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task<string> SendEmailFromTemplateAsync(
        string to,
        string templateName,
        Dictionary<string, string> templateVars,
        CancellationToken cancellationToken = default)
    {
        // Load template from file or database
        var subject = GetTemplateSubject(templateName);
        var body = GetTemplateBody(templateName, templateVars);

        return await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            return !string.IsNullOrEmpty(smtpHost);
        }
        catch
        {
            return false;
        }
    }

    private string GetTemplateSubject(string templateName) => templateName switch
    {
        "AppointmentReminder" => "Appointment Reminder",
        "AppointmentConfirmed" => "Appointment Confirmed",
        "AppointmentCancelled" => "Appointment Cancelled",
        "AppointmentRescheduled" => "Appointment Rescheduled",
        _ => "Appointment Notification"
    };

    private string GetTemplateBody(string templateName, Dictionary<string, string> vars)
    {
        var patientName = vars.TryGetValue("PatientName", out var pn) ? pn : "Patient";
        var appointmentDate = vars.TryGetValue("AppointmentDate", out var ad) ? ad : "upcoming appointment";
        var providerName = vars.TryGetValue("ProviderName", out var pr) ? pr : "your provider";

        return templateName switch
        {
            "AppointmentReminder" => $@"
                <h2>Appointment Reminder</h2>
                <p>Hi {patientName},</p>
                <p>This is a reminder of your appointment on <strong>{appointmentDate}</strong> with {providerName}.</p>
                <p>Please arrive 10 minutes early.</p>
                <p>Best regards,<br/>EHR Platform Team</p>",

            "AppointmentConfirmed" => $@"
                <h2>Appointment Confirmed</h2>
                <p>Hi {patientName},</p>
                <p>Your appointment with {providerName} on <strong>{appointmentDate}</strong> has been confirmed.</p>
                <p>Best regards,<br/>EHR Platform Team</p>",

            "AppointmentCancelled" => $@"
                <h2>Appointment Cancelled</h2>
                <p>Hi {patientName},</p>
                <p>Your appointment with {providerName} on <strong>{appointmentDate}</strong> has been cancelled.</p>
                <p>Best regards,<br/>EHR Platform Team</p>",

            _ => $@"
                <h2>Appointment Notification</h2>
                <p>Hi {patientName},</p>
                <p>You have a notification about your appointment on <strong>{appointmentDate}</strong>.</p>
                <p>Best regards,<br/>EHR Platform Team</p>"
        };
    }
}
