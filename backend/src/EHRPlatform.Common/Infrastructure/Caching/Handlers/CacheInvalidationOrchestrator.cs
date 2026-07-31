using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Orchestrates cache invalidation by routing events to domain-specific handlers.
/// Single responsibility: Route events to appropriate handlers.
/// </summary>
public class CacheInvalidationOrchestrator
{
    private readonly PatientCacheInvalidationHandler _patientHandler;
    private readonly AppointmentCacheInvalidationHandler _appointmentHandler;
    private readonly ClinicalCacheInvalidationHandler _clinicalHandler;
    private readonly UserCacheInvalidationHandler _userHandler;
    private readonly ReferenceDataCacheInvalidationHandler _refDataHandler;
    private readonly ILogger<CacheInvalidationOrchestrator> _logger;

    public CacheInvalidationOrchestrator(
        PatientCacheInvalidationHandler patientHandler,
        AppointmentCacheInvalidationHandler appointmentHandler,
        ClinicalCacheInvalidationHandler clinicalHandler,
        UserCacheInvalidationHandler userHandler,
        ReferenceDataCacheInvalidationHandler refDataHandler,
        ILogger<CacheInvalidationOrchestrator> logger)
    {
        _patientHandler = patientHandler ?? throw new ArgumentNullException(nameof(patientHandler));
        _appointmentHandler = appointmentHandler ?? throw new ArgumentNullException(nameof(appointmentHandler));
        _clinicalHandler = clinicalHandler ?? throw new ArgumentNullException(nameof(clinicalHandler));
        _userHandler = userHandler ?? throw new ArgumentNullException(nameof(userHandler));
        _refDataHandler = refDataHandler ?? throw new ArgumentNullException(nameof(refDataHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle cache invalidation event by routing to appropriate domain handler.
    /// </summary>
    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (eventType)
            {
                case "PatientCreated":
                case "PatientUpdated":
                case "PatientDeleted":
                    await _patientHandler.HandleEventAsync(eventType, eventData, cancellationToken);
                    break;

                case "AppointmentCreated":
                case "AppointmentUpdated":
                case "AppointmentCancelled":
                    await _appointmentHandler.HandleEventAsync(eventType, eventData, cancellationToken);
                    break;

                case "SoapNoteCreated":
                case "SoapNoteUpdated":
                case "SoapNoteDeleted":
                case "VitalsUpdated":
                case "DiagnosisAdded":
                case "DiagnosisUpdated":
                case "AllergyAdded":
                case "AllergyUpdated":
                    await _clinicalHandler.HandleEventAsync(eventType, eventData, cancellationToken);
                    break;

                case "UserCreated":
                case "UserUpdated":
                case "UserDeleted":
                case "RoleAssigned":
                case "RoleRevoked":
                case "PermissionChanged":
                    await _userHandler.HandleEventAsync(eventType, eventData, cancellationToken);
                    break;

                case "ReferenceDataUpdated":
                case "ReferenceDataDeleted":
                    await _refDataHandler.HandleEventAsync(eventType, eventData, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unknown cache invalidation event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cache invalidation orchestrator for event: {EventType}", eventType);
        }
    }
}
