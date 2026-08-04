using System;
using System.Diagnostics;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Custom application metrics for EHR platform.
/// Tracks business-relevant metrics for monitoring and alerting.
/// </summary>
public class ApplicationMetrics
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("EHRPlatform");
    
    // Counters
    private static readonly System.Diagnostics.Metrics.Meter Meter = new System.Diagnostics.Metrics.Meter("EHRPlatform.Metrics", "1.0.0");

    // Patient metrics
    private readonly System.Diagnostics.Metrics.Counter<long> _patientCreatedCounter;
    private readonly System.Diagnostics.Metrics.Counter<long> _patientDeletedCounter;

    // Appointment metrics
    private readonly System.Diagnostics.Metrics.Counter<long> _appointmentScheduledCounter;
    private readonly System.Diagnostics.Metrics.Counter<long> _appointmentCancelledCounter;

    // Clinical metrics
    private readonly System.Diagnostics.Metrics.Counter<long> _clinicalNoteCreatedCounter;

    // Billing metrics
    private readonly System.Diagnostics.Metrics.Counter<long> _invoiceGeneratedCounter;
    private readonly System.Diagnostics.Metrics.Counter<long> _paymentProcessedCounter;

    public ApplicationMetrics()
    {
        _patientCreatedCounter = Meter.CreateCounter<long>("ehr.patient.created", description: "Number of patients created");
        _patientDeletedCounter = Meter.CreateCounter<long>("ehr.patient.deleted", description: "Number of patients deleted");

        _appointmentScheduledCounter = Meter.CreateCounter<long>("ehr.appointment.scheduled", description: "Number of appointments scheduled");
        _appointmentCancelledCounter = Meter.CreateCounter<long>("ehr.appointment.cancelled", description: "Number of appointments cancelled");

        _clinicalNoteCreatedCounter = Meter.CreateCounter<long>("ehr.clinical.note.created", description: "Number of clinical notes created");

        _invoiceGeneratedCounter = Meter.CreateCounter<long>("ehr.invoice.generated", description: "Number of invoices generated");
        _paymentProcessedCounter = Meter.CreateCounter<long>("ehr.payment.processed", description: "Number of payments processed");
    }

    /// <summary>
    /// Record patient creation.
    /// </summary>
    public void RecordPatientCreated(string patientId)
    {
        _patientCreatedCounter.Add(1, new System.Collections.Generic.KeyValuePair<string, object?>("patientId", patientId));
    }

    /// <summary>
    /// Record patient deletion.
    /// </summary>
    public void RecordPatientDeleted(string patientId)
    {
        _patientDeletedCounter.Add(1, new System.Collections.Generic.KeyValuePair<string, object?>("patientId", patientId));
    }

    /// <summary>
    /// Record appointment scheduled.
    /// </summary>
    public void RecordAppointmentScheduled(string appointmentId, string providerId)
    {
        _appointmentScheduledCounter.Add(1,
            new System.Collections.Generic.KeyValuePair<string, object?>("appointmentId", appointmentId),
            new System.Collections.Generic.KeyValuePair<string, object?>("providerId", providerId));
    }

    /// <summary>
    /// Record appointment cancelled.
    /// </summary>
    public void RecordAppointmentCancelled(string appointmentId)
    {
        _appointmentCancelledCounter.Add(1, new System.Collections.Generic.KeyValuePair<string, object?>("appointmentId", appointmentId));
    }

    /// <summary>
    /// Record clinical note created.
    /// </summary>
    public void RecordClinicalNoteCreated(string patientId, string noteType)
    {
        _clinicalNoteCreatedCounter.Add(1,
            new System.Collections.Generic.KeyValuePair<string, object?>("patientId", patientId),
            new System.Collections.Generic.KeyValuePair<string, object?>("noteType", noteType));
    }

    /// <summary>
    /// Record invoice generated.
    /// </summary>
    public void RecordInvoiceGenerated(string invoiceId, decimal amount)
    {
        _invoiceGeneratedCounter.Add(1,
            new System.Collections.Generic.KeyValuePair<string, object?>("invoiceId", invoiceId),
            new System.Collections.Generic.KeyValuePair<string, object?>("amount", amount));
    }

    /// <summary>
    /// Record payment processed.
    /// </summary>
    public void RecordPaymentProcessed(string paymentId, decimal amount, string status)
    {
        _paymentProcessedCounter.Add(1,
            new System.Collections.Generic.KeyValuePair<string, object?>("paymentId", paymentId),
            new System.Collections.Generic.KeyValuePair<string, object?>("amount", amount),
            new System.Collections.Generic.KeyValuePair<string, object?>("status", status));
    }

    /// <summary>
    /// Create activity for distributed tracing.
    /// </summary>
    public static Activity? StartActivity(string operationName)
    {
        return ActivitySource.StartActivity(operationName);
    }

    /// <summary>
    /// Get activity source for custom instrumentation.
    /// </summary>
    public static ActivitySource GetActivitySource()
    {
        return ActivitySource;
    }

    /// <summary>
    /// Generic counter increment by metric name
    /// </summary>
    public void IncrementCounter(string metricName, long value = 1)
    {
        // Route to appropriate counter based on metric name
        switch (metricName.ToLowerInvariant())
        {
            case "patientcreated" or "ehr.patient.created":
                _patientCreatedCounter.Add(value);
                break;
            case "patientdeleted" or "ehr.patient.deleted":
                _patientDeletedCounter.Add(value);
                break;
            case "appointmentscheduled" or "ehr.appointment.scheduled":
                _appointmentScheduledCounter.Add(value);
                break;
            case "appointmentcancelled" or "ehr.appointment.cancelled":
                _appointmentCancelledCounter.Add(value);
                break;
            case "clinicalnotecreated" or "ehr.clinical.note.created":
                _clinicalNoteCreatedCounter.Add(value);
                break;
            case "invoicegenerated" or "ehr.invoice.generated":
                _invoiceGeneratedCounter.Add(value);
                break;
            case "paymentprocessed" or "ehr.payment.processed":
                _paymentProcessedCounter.Add(value);
                break;
        }
    }
}
