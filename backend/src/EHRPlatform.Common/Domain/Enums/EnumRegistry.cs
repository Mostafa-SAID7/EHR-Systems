#nullable enable

using System.Collections.Concurrent;

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Centralized registry of all service enums across EHR platform.
/// Provides discovery, mapping, slug generation, and validation for 21 distributed enums.
///
/// Inventory (21 total):
/// Appointment (3): AppointmentStatus, AppointmentType, ReminderType
/// Billing (4): InvoiceStatus, ClaimStatus, PriorAuthStatus, PaymentMethod
/// Clinical (4): ClinicalNoteStatus, ClinicalNoteType, DiagnosisType, EncounterType
/// Notification (3): NotificationStatus, NotificationType, NotificationChannel
/// Identity (2): RoleType, UserStatus
/// Audit (3): ActionType, AuditStatus, AccessLevel
/// Analytics (1): MetricType
/// Common (1): EncryptionStatus
/// Prescription (0): [Will add if categorization needed]
/// </summary>
public sealed class EnumRegistry
{
    private static readonly Lazy<EnumRegistry> _instance = new(() => new EnumRegistry());
    private readonly ConcurrentDictionary<string, EnumMetadata> _registry = new();

    public static EnumRegistry Instance => _instance.Value;

    private EnumRegistry()
    {
        InitializeRegistry();
    }

    /// <summary>
    /// Register all known service enums.
    /// </summary>
    private void InitializeRegistry()
    {
        // ── Appointment Service ────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Appointment.Domain.Enums.AppointmentStatus",
            "AppointmentStatus",
            "Appointment",
            EnumCategoryType.Status,
            "Lifecycle status of an appointment (Scheduled, Completed, Cancelled, etc.)");

        RegisterEnum("EHRPlatform.Services.Appointment.Domain.Enums.AppointmentType",
            "AppointmentType",
            "Appointment",
            EnumCategoryType.Type,
            "Type of appointment (Office, Telehealth, Phone)");

        RegisterEnum("EHRPlatform.Services.Appointment.Domain.Enums.ReminderType",
            "ReminderType",
            "Appointment",
            EnumCategoryType.Channel,
            "Reminder delivery method (Email, SMS, InApp)");

        // ── Billing Service ────────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Billing.Domain.Enums.InvoiceStatus",
            "InvoiceStatus",
            "Billing",
            EnumCategoryType.Status,
            "Invoice lifecycle status (Draft, Submitted, Paid, Overdue, etc.)");

        RegisterEnum("EHRPlatform.Services.Billing.Domain.Enums.ClaimStatus",
            "ClaimStatus",
            "Billing",
            EnumCategoryType.Status,
            "Insurance claim processing status");

        RegisterEnum("EHRPlatform.Services.Billing.Domain.Enums.PriorAuthStatus",
            "PriorAuthStatus",
            "Billing",
            EnumCategoryType.Status,
            "Prior authorization approval status");

        RegisterEnum("EHRPlatform.Services.Billing.Domain.Enums.PaymentMethod",
            "PaymentMethod",
            "Billing",
            EnumCategoryType.Method,
            "Payment method (CreditCard, Check, ACH, Insurance)");

        // ── Clinical Service ───────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Clinical.Domain.Enums.ClinicalNoteStatus",
            "ClinicalNoteStatus",
            "Clinical",
            EnumCategoryType.Status,
            "Clinical note lifecycle (Draft, Finalized, Locked)");

        RegisterEnum("EHRPlatform.Services.Clinical.Domain.Enums.ClinicalNoteType",
            "ClinicalNoteType",
            "Clinical",
            EnumCategoryType.Type,
            "Type of clinical note (SOAP, Progress, Consultation, Discharge, FollowUp)");

        RegisterEnum("EHRPlatform.Services.Clinical.Domain.Enums.DiagnosisType",
            "DiagnosisType",
            "Clinical",
            EnumCategoryType.Classification,
            "Diagnosis classification (Principal, Secondary, RuleOut)");

        RegisterEnum("EHRPlatform.Services.Clinical.Domain.Enums.EncounterType",
            "EncounterType",
            "Clinical",
            EnumCategoryType.Type,
            "Type of clinical encounter (Office, Telehealth, Emergency, Hospital)");

        // ── Notification Service ───────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Notification.Domain.Enums.NotificationStatus",
            "NotificationStatus",
            "Notification",
            EnumCategoryType.Status,
            "Notification delivery status (Pending, Sent, Failed, Bounced)");

        RegisterEnum("EHRPlatform.Services.Notification.Domain.Enums.NotificationType",
            "NotificationType",
            "Notification",
            EnumCategoryType.Type,
            "Notification type/trigger (Appointment, Prescription, Billing, Clinical, Alert)");

        RegisterEnum("EHRPlatform.Services.Notification.Domain.Enums.NotificationChannel",
            "NotificationChannel",
            "Notification",
            EnumCategoryType.Channel,
            "Notification delivery channel (Email, SMS, Push, InApp)");

        // ── Identity Service ───────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Identity.Domain.Enums.RoleType",
            "RoleType",
            "Identity",
            EnumCategoryType.Role,
            "User role classification (Admin, Doctor, Nurse, Patient, Billing, IT)");

        RegisterEnum("EHRPlatform.Services.Identity.Domain.Enums.UserStatus",
            "UserStatus",
            "Identity",
            EnumCategoryType.Status,
            "User account status (Active, Inactive, Suspended, Deleted)");

        // ── Audit Service ──────────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Audit.Domain.Enums.ActionType",
            "ActionType",
            "Audit",
            EnumCategoryType.Action,
            "Audited action type (Create, Update, Delete, Read, Execute)");

        RegisterEnum("EHRPlatform.Services.Audit.Domain.Enums.AuditStatus",
            "AuditStatus",
            "Audit",
            EnumCategoryType.Result,
            "Audit entry result status (Success, Failure, Warning)");

        RegisterEnum("EHRPlatform.Services.Audit.Domain.Enums.AccessLevel",
            "AccessLevel",
            "Audit",
            EnumCategoryType.Level,
            "Data access sensitivity level (Public, Internal, Confidential, Secret)");

        // ── Analytics Service ──────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Services.Analytics.Domain.Enums.MetricType",
            "MetricType",
            "Analytics",
            EnumCategoryType.Classification,
            "Analytics metric type (PatientCount, AppointmentVolume, BillingAmount, Compliance)");

        // ── Common Library ─────────────────────────────────────────────────────
        RegisterEnum("EHRPlatform.Common.Enums.EncryptionStatus",
            "EncryptionStatus",
            "Common",
            EnumCategoryType.Status,
            "Data encryption status (Encrypted, Unencrypted, Partial)");
    }

    /// <summary>
    /// Register a single enum in the registry.
    /// </summary>
    private void RegisterEnum(
        string fullTypeName,
        string displayName,
        string serviceName,
        EnumCategoryType category,
        string? description = null)
    {
        var metadata = new EnumMetadata
        {
            FullTypeName = fullTypeName,
            DisplayName = displayName,
            ServiceName = serviceName,
            Category = category,
            Description = description
        };

        _registry[fullTypeName] = metadata;
    }

    /// <summary>
    /// Get all registered enums.
    /// </summary>
    public IEnumerable<EnumMetadata> GetAll() => _registry.Values;

    /// <summary>
    /// Get all enums in a specific category.
    /// </summary>
    public IEnumerable<EnumMetadata> GetByCategory(EnumCategoryType category)
    {
        return _registry.Values.Where(m => m.Category == category);
    }

    /// <summary>
    /// Get all enums belonging to a service.
    /// </summary>
    public IEnumerable<EnumMetadata> GetByService(string serviceName)
    {
        return _registry.Values.Where(m => 
            m.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get enum metadata by full type name.
    /// </summary>
    public EnumMetadata? GetMetadata(string fullTypeName)
    {
        _registry.TryGetValue(fullTypeName, out var metadata);
        return metadata;
    }

    /// <summary>
    /// Get enum metadata by short display name.
    /// </summary>
    public EnumMetadata? GetMetadataByName(string displayName)
    {
        return _registry.Values.FirstOrDefault(m => 
            m.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get a summary report of all registered enums.
    /// </summary>
    public string GetSummaryReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== EHR Platform Enum Registry Summary ===");
        report.AppendLine($"Total Enums: {_registry.Count}");
        report.AppendLine();

        // Group by service
        var byService = _registry.Values.GroupBy(m => m.ServiceName).OrderBy(g => g.Key);
        foreach (var serviceGroup in byService)
        {
            report.AppendLine($"[{serviceGroup.Key} Service] ({serviceGroup.Count()} enums)");
            foreach (var meta in serviceGroup.OrderBy(m => m.DisplayName))
            {
                report.AppendLine($"  • {meta.DisplayName} ({meta.Category})");
                if (meta.Description != null)
                    report.AppendLine($"    └─ {meta.Description}");
            }
            report.AppendLine();
        }

        // Group by category
        report.AppendLine("=== By Category ===");
        var byCategory = _registry.Values.GroupBy(m => m.Category).OrderBy(g => g.Key);
        foreach (var catGroup in byCategory)
        {
            report.AppendLine($"[{catGroup.Key}] ({catGroup.Count()} enums)");
            foreach (var meta in catGroup.OrderBy(m => m.DisplayName))
                report.AppendLine($"  • {meta.DisplayName} ({meta.ServiceName})");
            report.AppendLine();
        }

        return report.ToString();
    }
}

