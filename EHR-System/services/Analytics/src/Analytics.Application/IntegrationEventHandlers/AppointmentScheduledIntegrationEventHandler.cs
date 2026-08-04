namespace EHRPlatform.Services.Analytics.Application.IntegrationEventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Integration event handler for when appointment is scheduled in Appointment service
/// </summary>
public class AppointmentScheduledIntegrationEventHandler : INotificationHandler<AppointmentScheduledIntegrationEvent>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AppointmentScheduledIntegrationEventHandler> _logger;

    public AppointmentScheduledIntegrationEventHandler(
        IMetricRepository metricRepository,
        ITenantContext tenantContext,
        ILogger<AppointmentScheduledIntegrationEventHandler> logger)
    {
        _metricRepository = metricRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Handle(AppointmentScheduledIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing AppointmentScheduled event: {AppointmentId}", notification.AppointmentId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Record metric for scheduled appointment
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "AppointmentScheduled",
                Category = "Appointments",
                Value = 1,
                Unit = "count",
                Timestamp = notification.ScheduledDate,
                Dimension1 = notification.DoctorId.ToString(),
                Dimension2 = notification.PatientId.ToString(),
                Dimension3 = notification.ClinicId.ToString(),
                TenantId = tenantId
            };

            await _metricRepository.AddAsync(metric);

            _logger.LogInformation("Recorded AppointmentScheduled metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AppointmentScheduled event: {AppointmentId}", 
                notification.AppointmentId);
        }
    }
}

/// <summary>
/// Integration event from Appointment service
/// </summary>
public class AppointmentScheduledIntegrationEvent : INotification
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string ClinicId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public long TenantId { get; set; }
}
