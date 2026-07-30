using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

public class AggregateMetricsCommandHandler : ICommandHandler<AggregateMetricsCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AggregateMetricsCommandHandler> _logger;

    public AggregateMetricsCommandHandler(IUnitOfWork unitOfWork, ILogger<AggregateMetricsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AggregateMetricsCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Aggregating metrics for {Frequency}", command.Frequency);
        var (periodStart, periodEnd) = GetPeriodDates(command.Frequency, command.ForPeriod ?? DateTime.UtcNow);

        var eventRepo = _unitOfWork.Repository<EventMetric>();
        var events = await eventRepo.ToListAsync(
            q => q.Where(e => e.OccurredAt >= periodStart && e.OccurredAt < periodEnd), ct);

        var aggregates = events.GroupBy(e => e.EventType)
            .Select(g => new AnalyticsMetric
            {
                Id = Guid.NewGuid(), 
                MetricName = $"{g.Key}_count",
                Category = GetCategory(g.Key),
                PeriodStart = periodStart, 
                PeriodEnd = periodEnd,
                Value = g.Count(), 
                Unit = "count", 
                Frequency = command.Frequency
            }).ToList();

        var metricRepo = _unitOfWork.Repository<AnalyticsMetric>();
        foreach (var agg in aggregates)
            await metricRepo.AddAsync(agg, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Aggregated {Count} metrics", aggregates.Count);
    }

    private (DateTime, DateTime) GetPeriodDates(string frequency, DateTime date) => frequency.ToLower() switch
    {
        "weekly"  => (date.AddDays(-(int)date.DayOfWeek).Date, date.AddDays(-(int)date.DayOfWeek).Date.AddDays(7)),
        "monthly" => (new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, 1).AddMonths(1)),
        _         => (date.Date, date.Date.AddDays(1))
    };

    private static string GetCategory(string eventType) => eventType switch
    {
        "PatientCreated" or "PatientUpdated"               => "Patients",
        "AppointmentScheduled" or "AppointmentCompleted"   => "Appointments",
        "InvoiceCreated" or "PaymentReceived"              => "Revenue",
        "ClinicalNoteCreated"                              => "Clinical",
        _                                                  => "System"
    };
}

