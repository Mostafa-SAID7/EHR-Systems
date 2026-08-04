namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// AnalyticsMetric - KPI and business metric aggregation.
/// Aggregated from Kafka domain events.
/// </summary>
public class AnalyticsMetric
{
    public Guid Id { get; set; }
    public string MetricName { get; set; } = string.Empty; // patient_count, appointment_completed, revenue_paid, etc.
    public string Category { get; set; } = string.Empty; // Patients, Appointments, Clinical, Billing, etc.
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty; // count, minutes, dollars, percentage
    public DateTime Timestamp { get; set; }
    public DateTime? MetricDate { get; set; } // For daily aggregations
    
    // Dimensions for filtering
    public string? Dimension1 { get; set; } // Provider, Department, Location, Status, etc.
    public string? Dimension2 { get; set; }
    public string? Dimension3 { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

