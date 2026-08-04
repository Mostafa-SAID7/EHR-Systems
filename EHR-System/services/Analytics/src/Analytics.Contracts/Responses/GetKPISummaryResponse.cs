namespace EHRPlatform.Services.Analytics.Contracts.Responses;

public class GetKPISummaryResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public KPISummaryDto? Summary { get; set; }
}

public class KPISummaryDto
{
    public DateTime SummaryDate { get; set; }
    public int TotalPatients { get; set; }
    public int NewPatients { get; set; }
    public int AppointmentsScheduled { get; set; }
    public int AppointmentsCompleted { get; set; }
    public int AppointmentsCancelled { get; set; }
    public double AverageAppointmentDurationMinutes { get; set; }
    public int ClinicalNotesCreated { get; set; }
    public decimal RevenueInvoiced { get; set; }
    public decimal RevenuePaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public double SystemUptime { get; set; }
    public int ApiCallCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
}
