namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when prescription is created in Pharmacy service
/// </summary>
public class PrescriptionCreatedIntegrationEventHandler : INotificationHandler<PrescriptionCreatedIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PrescriptionCreatedIntegrationEventHandler> _logger;

    public PrescriptionCreatedIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<PrescriptionCreatedIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(PrescriptionCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing PrescriptionCreated event: {PrescriptionId}", notification.PrescriptionId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for prescription
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "PrescriptionCreated",
                Category = "Pharmacy",
                Value = 1,
                Unit = "count",
                Timestamp = notification.CreatedDate,
                Dimension1 = notification.PatientId.ToString(),
                Dimension2 = notification.DoctorId.ToString(),
                Dimension3 = notification.MedicationCode,
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded PrescriptionCreated metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PrescriptionCreated event: {PrescriptionId}", 
                notification.PrescriptionId);
        }
    }
}

/// <summary>
/// Integration event from Pharmacy service
/// </summary>
public class PrescriptionCreatedIntegrationEvent : INotification
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string MedicationCode { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public long TenantId { get; set; }
}
