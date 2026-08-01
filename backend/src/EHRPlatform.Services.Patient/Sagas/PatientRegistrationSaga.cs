using EHRPlatform.BuildingBlocks.Observability.Telemetry;
using EHRPlatform.Services.Patient.Domain.Events;
using EHRPlatform.Services.Patient.Messaging.Messages;
using EHRPlatform.Services.Patient.Sagas.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Sagas;

/// <summary>
/// MassTransit Saga State Machine: coordinates the patient registration workflow
/// across multiple microservices using the Orchestration pattern.
///
/// Flow:
///   1. PatientCreated (Kafka)
///      → send SendWelcomeNotificationMessage (RabbitMQ)
///      → send PatientIndexMessage (RabbitMQ)
///      → (in full system) request billing account creation
///   2. WelcomeNotificationSentEvent → mark notification done
///   3. PatientIndexedEvent          → mark search indexed
///   4. All steps done               → transition to Completed
///   5. Any failure                  → transition to Failed + compensate
///
/// HIPAA: saga state is persisted in database for full auditability.
/// Each transition is timestamped and correlated.
/// </summary>
public sealed class PatientRegistrationSaga :
    MassTransitStateMachine<PatientRegistrationSagaState>
{
    private readonly ILogger<PatientRegistrationSaga> _logger;

    // ── States ────────────────────────────────────────────────────────────────
    public State Registered { get; private set; } = null!;
    public State ProcessingSteps { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // ── Events ────────────────────────────────────────────────────────────────
    public Event<PatientCreatedEvent> PatientCreated { get; private set; } = null!;
    public Event<WelcomeNotificationSentEvent> NotificationSent { get; private set; } = null!;
    public Event<PatientIndexedEvent> PatientIndexed { get; private set; } = null!;
    public Event<PatientRegistrationFailedEvent> RegistrationFailed { get; private set; } = null!;

    public PatientRegistrationSaga(ILogger<PatientRegistrationSaga> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // ── Correlate all events by PatientId ──────────────────────────────
        Event(() => PatientCreated, x => x.CorrelateById(ctx => ctx.Message.PatientId));
        Event(() => NotificationSent, x => x.CorrelateById(ctx => ctx.Message.PatientId));
        Event(() => PatientIndexed, x => x.CorrelateById(ctx => ctx.Message.PatientId));
        Event(() => RegistrationFailed, x => x.CorrelateById(ctx => ctx.Message.PatientId));

        // ── State Machine Definition ───────────────────────────────────────
        Initially(
            When(PatientCreated)
                .Then(ctx =>
                {
                    var evt = ctx.Message;
                    ctx.Saga.PatientId  = evt.PatientId;
                    ctx.Saga.MRN        = evt.MRN;
                    ctx.Saga.Email      = evt.Email;
                    ctx.Saga.FirstName  = evt.FirstName;
                    ctx.Saga.LastName   = evt.LastName;
                    ctx.Saga.TenantId   = evt.TenantId;
                    ctx.Saga.UpdatedAt  = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Saga started for PatientId={PatientId}",
                        evt.PatientId);
                })
                .Publish(ctx => new SendWelcomeNotificationMessage
                {
                    PatientId    = ctx.Saga.PatientId,
                    FirstName    = ctx.Saga.FirstName,
                    LastName     = ctx.Saga.LastName,
                    Email        = ctx.Saga.Email,
                    MRN          = ctx.Saga.MRN,
                    TenantId     = ctx.Saga.TenantId,
                    RegisteredAt = DateTime.UtcNow
                })
                .Publish(ctx => new PatientIndexMessage
                {
                    PatientId = ctx.Saga.PatientId,
                    FirstName = ctx.Saga.FirstName,
                    LastName  = ctx.Saga.LastName,
                    Email     = ctx.Saga.Email,
                    MRN       = ctx.Saga.MRN,
                    Status    = "Active"
                })
                .TransitionTo(ProcessingSteps));

        During(ProcessingSteps,
            When(NotificationSent)
                .Then(ctx =>
                {
                    ctx.Saga.WelcomeNotificationSent = true;
                    ctx.Saga.NotificationSentAt = DateTime.UtcNow;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation(
                        "Notification sent for PatientId={PatientId}",
                        ctx.Saga.PatientId);
                })
                .IfElse(ctx => ctx.Saga.SearchIndexed,
                    complete => complete.TransitionTo(Completed)
                                        .Then(MarkCompleted),
                    pending => pending.TransitionTo(ProcessingSteps)),

            When(PatientIndexed)
                .Then(ctx =>
                {
                    ctx.Saga.SearchIndexed = true;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation(
                        "Search indexed for PatientId={PatientId}",
                        ctx.Saga.PatientId);
                })
                .IfElse(ctx => ctx.Saga.WelcomeNotificationSent,
                    complete => complete.TransitionTo(Completed)
                                        .Then(MarkCompleted),
                    pending => pending.TransitionTo(ProcessingSteps)),

            When(RegistrationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogError(
                        "PatientRegistrationSaga FAILED for PatientId={PatientId}: {Reason}",
                        ctx.Saga.PatientId, ctx.Message.Reason);
                })
                .TransitionTo(Failed));

        SetCompletedWhenFinalized();
    }

    private static void MarkCompleted(BehaviorContext<PatientRegistrationSagaState> ctx)
    {
        ctx.Saga.CompletedAt = DateTime.UtcNow;
        ctx.Saga.UpdatedAt = DateTime.UtcNow;
    }
}


