using System;
using System.Collections.Generic;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Analytics Report Communication
    /// </summary>
    public class AnalyticsReportDto
    {
        public Guid Id { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; }      // e.g., "Patient Metrics", "Revenue", "Appointment", "Clinical Outcomes"
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }          // e.g., "Generating", "Ready", "Failed"
    }

    /// <summary>
    /// Shared DTO for Metric Data
    /// </summary>
    public class MetricDataDto
    {
        public string MetricName { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    /// <summary>
    /// Event: Report Generated
    /// Published by Analytics Service when report is completed
    /// Subscribed by: Admin dashboard, Compliance, Audit
    /// </summary>
    public class ReportGeneratedEvent
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RecordCount { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Metrics Aggregated
    /// Published by Analytics Service when metrics are calculated
    /// Subscribed by: Dashboard, Real-time monitoring
    /// </summary>
    public class MetricsAggregatedEvent
    {
        public Guid ReportId { get; set; }
        public string MetricCategory { get; set; }  // e.g., "Patient", "Revenue", "Appointment"
        public Dictionary<string, double> Metrics { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Analytics Alert Generated
    /// Published by Analytics Service when anomaly is detected
    /// Subscribed by: Management alerts, Compliance team
    /// </summary>
    public class AnalyticsAlertGeneratedEvent
    {
        public Guid AlertId { get; set; }
        public string AlertType { get; set; }       // e.g., "Revenue Decline", "Appointment No-show Rate", "Patient Readmission"
        public string Severity { get; set; }        // e.g., "Low", "Medium", "High", "Critical"
        public string Message { get; set; }
        public Dictionary<string, object> Details { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Scheduled Report Exported
    /// Published by Analytics Service when report is exported (PDF, Excel, etc.)
    /// Subscribed by: Email delivery, Audit
    /// </summary>
    public class ScheduledReportExportedEvent
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; }
        public string FileFormat { get; set; }      // e.g., "PDF", "XLSX", "CSV"
        public string ExportPath { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
