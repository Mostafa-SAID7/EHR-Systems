using EHRPlatform.BuildingBlocks.Common.Sagas;
using MassTransit;

namespace EHRPlatform.Services.Patient.Sagas;

/// <summary>
/// Persistent state for the PatientRegistration distributed saga.
/// Stored in the patient PostgreSQL database (PatientContext).
///
/// State machine transitions:
///   Initial → Registered → BillingPending → NotificationPending → Completed
///                                         ↘ Failed (compensation)
/// </summary>
public class PatientRegistrationSagaState : SagaStateBase, SagaStateMachineInstance
{
    // ── MassTransit saga identity ─────────────────────────────────────────────
    // CorrelationId comes from SagaStateBase

    // ── Patient data captured at saga start ──────────────────────────────────
    public Guid PatientId { get; set; }
    public string MRN { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }

    // ── Step completion flags (for idempotency on replay) ───────────────────
    public bool BillingAccountCreated { get; set; }
    public bool SearchIndexed { get; set; }
    public bool WelcomeNotificationSent { get; set; }

    // ── Timing ────────────────────────────────────────────────────────────────
    public DateTime? BillingCompletedAt { get; set; }
    public DateTime? NotificationSentAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ── Compensation ──────────────────────────────────────────────────────────
    public string? FailureReason { get; set; }
    public bool CompensationExecuted { get; set; }
}

