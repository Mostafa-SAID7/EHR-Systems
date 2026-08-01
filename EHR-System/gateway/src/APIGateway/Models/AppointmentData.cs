namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Appointment data from Appointment Service.
/// </summary>
public class AppointmentData
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
