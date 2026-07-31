#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Telemetry and tracing constants for the EHR platform.
/// Includes service identification and well-known tag names for OpenTelemetry.
/// Single responsibility: Define telemetry constants only.
/// </summary>
public static class TelemetryConstants
{
    /// <summary>Service name identifier.</summary>
    public const string ServiceName = "ehr-platform";

    /// <summary>Service version.</summary>
    public const string ServiceVersion = "1.0.0";

    // ── Well-known OpenTelemetry tag names ─────────────────────────────────

    /// <summary>Tag for patient ID in distributed traces.</summary>
    public const string TagPatientId = "ehr.patient.id";

    /// <summary>Tag for tenant/organization ID in distributed traces.</summary>
    public const string TagTenantId = "ehr.tenant.id";

    /// <summary>Tag for correlation ID in distributed traces.</summary>
    public const string TagCorrelationId = "ehr.correlation.id";

    /// <summary>Tag for event type in distributed traces.</summary>
    public const string TagEventType = "ehr.event.type";

    /// <summary>Tag for saga ID in distributed traces.</summary>
    public const string TagSagaId = "ehr.saga.id";
}
