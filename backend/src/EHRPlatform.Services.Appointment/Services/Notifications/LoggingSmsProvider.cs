namespace EHRPlatform.Services.Appointment.Services.Notifications;

/// <summary>
/// Logging-based SMS provider implementation.
/// Logs SMS messages instead of sending them (useful for development/testing).
/// Can be replaced with Twilio, AWS SNS, or other real SMS providers.
/// </summary>
public class LoggingSmsProvider : ISmsProvider
{
    private readonly ILogger<LoggingSmsProvider> _logger;

    public LoggingSmsProvider(ILogger<LoggingSmsProvider> logger)
    {
        _logger = logger;
    }

    public async Task<string> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SMS (DEV/LOGGING): To {PhoneNumber} | Message: {Message}",
            phoneNumber, message);

        await Task.CompletedTask;
        return $"sms-dev-{Guid.NewGuid()}";
    }

    public async Task<string> SendSmsFromTemplateAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> templateVars,
        CancellationToken cancellationToken = default)
    {
        var patientName = templateVars.TryGetValue("PatientName", out var pn) ? pn : "Patient";
        var appointmentDate = templateVars.TryGetValue("AppointmentDate", out var ad) ? ad : "soon";

        var message = templateName switch
        {
            "AppointmentReminder" => $"Hi {patientName}, reminder: appointment on {appointmentDate}. Reply CONFIRM or CANCEL.",
            "AppointmentConfirmed" => $"Hi {patientName}, your appointment on {appointmentDate} is confirmed.",
            "AppointmentCancelled" => $"Hi {patientName}, your appointment on {appointmentDate} has been cancelled.",
            _ => $"Hi {patientName}, you have an appointment update."
        };

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<bool> IsAvailableAsync()
    {
        await Task.CompletedTask;
        return true; // Always available in dev/logging mode
    }
}
