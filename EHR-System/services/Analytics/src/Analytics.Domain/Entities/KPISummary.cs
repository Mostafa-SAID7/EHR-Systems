namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// KPISummary - Pre-calculated KPI snapshot
/// </summary>
public class KPISummary
{
    public Guid Id { get; set; }
    public DateTime SummaryDate { get; set; }
    
    // Patients
    public int TotalPatients { get; set; }
    public int NewPatients { get; set; }
    
    // Appointments
    public int AppointmentsScheduled { get; set; }
    public int AppointmentsCompleted { get; set; }
    public int AppointmentsCancelled { get; set; }
    public double AverageAppointmentDurationMinutes { get; set; }
    
    // Clinical
    public int ClinicalNotesCreated { get; set; }
    public int OrdersCreated { get; set; }
    
    // Billing
    public decimal RevenueInvoiced { get; set; }
    public decimal RevenuePaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    
    // System
    public double SystemUptime { get; set; } // percentage
    public int ApiCallCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
