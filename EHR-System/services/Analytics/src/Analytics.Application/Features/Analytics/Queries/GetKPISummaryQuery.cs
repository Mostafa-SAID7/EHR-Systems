namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;

/// <summary>
/// Query to get KPI summary (cached 15 minutes).
/// </summary>
public class GetKPISummaryQuery : IRequest<GetKPISummaryResponse>
{
    public DateTime? ForDate { get; set; }
}

public class GetKPISummaryResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public KPISummaryDto? Summary { get; set; }
}

public class KPISummaryDto
{
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
    
    // Billing
    public decimal RevenueInvoiced { get; set; }
    public decimal RevenuePaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    
    // System
    public double SystemUptime { get; set; }
    public int ApiCallCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
}
