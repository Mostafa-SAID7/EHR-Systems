namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when clinical note is created in Clinical Records service
/// </summary>
public class ClinicalNoteCreatedIntegrationEventHandler : INotificationHandler<ClinicalNoteCreatedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ClinicalNoteCreatedIntegrationEventHandler> _logger;

    public ClinicalNoteCreatedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<ClinicalNoteCreatedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(ClinicalNoteCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing ClinicalNoteCreated event: {NoteId}", notification.NoteId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for clinical note
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "ClinicalNoteCreated",
                Category = "Clinical",
                Value = 1,
                Unit = "count",
                Timestamp = notification.CreatedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.DoctorId.ToString(),
                Dimension3 = notification.NoteType,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded ClinicalNoteCreated metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ClinicalNoteCreated event: {NoteId}", 
                notification.NoteId);
        }
    }
}

/// <summary>
/// Integration event from Clinical Records service
/// </summary>
public class ClinicalNoteCreatedIntegrationEvent : INotification
{
    public Guid NoteId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string NoteType { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public long TenantId { get; set; }
}
