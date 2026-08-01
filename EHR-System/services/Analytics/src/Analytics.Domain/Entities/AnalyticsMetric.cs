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

/// <summary>
/// Dashboard - User-defined dashboards with widgets
/// </summary>
public class Dashboard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<DashboardWidget> Widgets { get; } = new List<DashboardWidget>();
}

/// <summary>
/// DashboardWidget - Widget on a dashboard
/// </summary>
public class DashboardWidget
{
    public Guid Id { get; set; }
    public Guid DashboardId { get; set; }
    public string WidgetType { get; set; } = string.Empty; // KPI, LineChart, BarChart, Table, Gauge
    public string MetricName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    public string? Configuration { get; set; } // JSON for chart config
    public DateTime CreatedAt { get; set; }

    public Dashboard Dashboard { get; set; } = null!;
}

/// <summary>
/// Report - Scheduled report definitions
/// </summary>
public class Report
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QueryDefinition { get; set; } = string.Empty; // JSON query
    public string ReportType { get; set; } = string.Empty; // Daily, Weekly, Monthly, OnDemand
    public string Status { get; set; } = "Active"; // Active, Inactive, Archived
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReportExecution> Executions { get; } = new List<ReportExecution>();
}

/// <summary>
/// ReportExecution - Report run execution tracking
/// </summary>
public class ReportExecution
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long DurationMs { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed, Running
    public int RecordCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputPath { get; set; } // S3/Blob storage path
    public string? ContentType { get; set; } // application/pdf, text/csv, application/json

    public Report Report { get; set; } = null!;
}

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

// Domain Events
public record MetricRecordedEvent(string MetricName, decimal Value, DateTime Timestamp)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record ReportExecutedEvent(Guid ReportId, string Status, int RecordCount)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
