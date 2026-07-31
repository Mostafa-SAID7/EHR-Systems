using System.Diagnostics;
using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Infrastructure.Telemetry;

/// <summary>
/// Central ActivitySource for EHR Platform distributed tracing.
///
/// All microservices use this source so traces are correlated under the same
/// instrumentation name in Jaeger / Zipkin / OTLP backends.
///
/// Usage:
///   using var activity = EHRTelemetry.StartActivity("HandleCreatePatient");
///   activity?.SetTag("patient.id", patientId);
///
/// HIPAA: Do NOT add PII (names, DOB, SSN) to trace tags.
/// Use opaque IDs only (PatientId, MRN hash).
/// </summary>
public static class EHRTelemetry
{
    /// <summary>Shared ActivitySource. Register once at startup via AddEHRTelemetry().</summary>
    public static readonly ActivitySource ActivitySource =
        new(TelemetryConstants.ServiceName, TelemetryConstants.ServiceVersion);

    // ── Well-known tag names ─────────────────────────────────────────────────
    // (Constants from TelemetryConstants)
    public const string TagUserId        = "ehr.user.id";

    /// <summary>
    /// Start a new activity (span) with standard EHR tags.
    /// Returns null-safe Activity so callers can use ?. throughout.
    /// </summary>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind = ActivityKind.Internal,
        string? correlationId = null,
        string? tenantId = null)
    {
        var activity = ActivitySource.StartActivity(name, kind);
        if (activity is null) return null;

        if (correlationId is not null)
            activity.SetTag(TagCorrelationId, correlationId);

        if (tenantId is not null)
            activity.SetTag(TagTenantId, tenantId);

        return activity;
    }

    /// <summary>Mark the current activity as failed.</summary>
    public static void RecordException(this Activity? activity, Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddEvent(new ActivityEvent("exception",
            tags: new ActivityTagsCollection
            {
                ["exception.type"]    = ex.GetType().FullName,
                ["exception.message"] = ex.Message
            }));
    }
}

