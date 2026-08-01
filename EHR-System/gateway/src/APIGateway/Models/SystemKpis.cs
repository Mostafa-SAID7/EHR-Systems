namespace EHRPlatform.Gateway.Models;

/// <summary>
/// System-wide KPIs from Analytics Service.
/// </summary>
public class SystemKpis
{
    public int TotalPatients { get; set; }
    public int TotalProviders { get; set; }
    public int AppointmentsThisMonth { get; set; }
    public int CompletedAppointmentsThisMonth { get; set; }
    public decimal AveragePatientSatisfaction { get; set; }
    public decimal TotalBillingThisMonth { get; set; }
    public int ActiveUsers { get; set; }
}
