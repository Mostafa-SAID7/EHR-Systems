namespace EHRPlatform.Common.Sagas;

/// <summary>
/// Marker interface for all EHR saga state machines.
/// Sagas coordinate distributed transactions across microservices
/// using the Choreography or Orchestration pattern.
///
/// Example: PatientRegistrationSaga
///   Start:   PatientCreated
///   Step 1:  TriggerBillingAccountCreation  → BillingAccountCreated
///   Step 2:  SendWelcomeNotification         → NotificationSent
///   Step 3:  IndexInElasticsearch            → (search updated)
///   End:     PatientRegistrationCompleted
///
/// Compensation: if any step fails, compensating transactions undo prior steps.
/// HIPAA: all state transitions are logged with correlation IDs for full auditability.
/// </summary>
public interface IEHRSaga
{
    /// <summary>Unique saga instance identifier.</summary>
    Guid CorrelationId { get; set; }

    /// <summary>Current saga state (e.g., "Pending", "Active", "Completed", "Failed").</summary>
    string CurrentState { get; set; }

    /// <summary>When this saga instance was created.</summary>
    DateTime CreatedAt { get; set; }

    /// <summary>When this saga instance last transitioned state.</summary>
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Base class for saga state data stored in the database.
/// Extend this for each saga type.
/// </summary>
public abstract class SagaStateBase : IEHRSaga
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Initial";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CorrelationContext { get; set; }
}
