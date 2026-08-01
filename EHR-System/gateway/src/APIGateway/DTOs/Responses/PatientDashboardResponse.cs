using EHRPlatform.Gateway.Models;

namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Patient dashboard response - aggregated data from multiple services.
/// </summary>
public class PatientDashboardResponse
{
    public string PatientId { get; set; } = string.Empty;
    public PatientData? Patient { get; set; }
    public List<AppointmentData> UpcomingAppointments { get; set; } = new();
    public BillingData? Billing { get; set; }
    public List<ClinicalNoteData> RecentClinicalNotes { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
