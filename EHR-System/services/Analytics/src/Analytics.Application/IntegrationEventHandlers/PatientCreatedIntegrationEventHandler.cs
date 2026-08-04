namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when patient is created in Patient service
/// </summary>
public class PatientCreatedIntegrationEventHandler : INotificationHandler<PatientCreatedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PatientCreatedIntegrationEventHandler> _logger;

    public PatientCreatedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<PatientCreatedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(PatientCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing PatientCreated event: {PatientId}", notification.PatientId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for new patient
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "NewPatient",
                Category = "Patients",
                Value = 1,
                Unit = "count",
                Timestamp = notification.CreatedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.PatientAge.ToString(),
                Dimension3 = notification.PatientStatus,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded NewPatient metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PatientCreated event: {PatientId}", 
                notification.PatientId);
        }
    }
}

/// <summary>
/// Integration event from Patient service
/// </summary>
public class PatientCreatedIntegrationEvent : INotification
{
    public Guid PatientId { get; set; }
    public int PatientAge { get; set; }
    public string PatientStatus { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public long TenantId { get; set; }
}
