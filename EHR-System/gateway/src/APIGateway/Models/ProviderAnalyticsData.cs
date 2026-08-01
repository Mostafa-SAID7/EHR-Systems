namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Provider analytics data from Analytics Service.
/// </summary>
public class ProviderAnalyticsData
{
    public string ProviderId { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
