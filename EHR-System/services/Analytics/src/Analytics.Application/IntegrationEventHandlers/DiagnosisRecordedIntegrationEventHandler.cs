namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when diagnosis is recorded in Clinical Records service
/// </summary>
public class DiagnosisRecordedIntegrationEventHandler : INotificationHandler<DiagnosisRecordedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DiagnosisRecordedIntegrationEventHandler> _logger;

    public DiagnosisRecordedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<DiagnosisRecordedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(DiagnosisRecordedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing DiagnosisRecorded event: {DiagnosisId}", notification.DiagnosisId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for diagnosis
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "DiagnosisRecorded",
                Category = "Clinical",
                Value = 1,
                Unit = "count",
                Timestamp = notification.RecordedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.DoctorId.ToString(),
                Dimension3 = notification.DiagnosisCode,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded DiagnosisRecorded metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DiagnosisRecorded event: {DiagnosisId}", 
                notification.DiagnosisId);
        }
    }
}

/// <summary>
/// Integration event from Clinical Records service
/// </summary>
public class DiagnosisRecordedIntegrationEvent : INotification
{
    public Guid DiagnosisId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string DiagnosisCode { get; set; } = string.Empty;
    public DateTime RecordedDate { get; set; }
    public long TenantId { get; set; }
}
