using EHRPlatform.Gateway.Models;

namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Provider dashboard response - aggregated appointment and analytics data.
/// </summary>
public class ProviderDashboardResponse
{
    public string ProviderId { get; set; } = string.Empty;
    public List<AppointmentData> TodayAppointments { get; set; } = new();
    public List<AppointmentData> UpcomingAppointments { get; set; } = new();
    public ProviderAnalyticsData? Analytics { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
