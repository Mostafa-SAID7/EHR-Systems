namespace EHRPlatform.Services.Analytics.Domain.Services;

using EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// Domain service for calculating KPI metrics
/// Aggregates raw metrics into KPI summaries
/// </summary>
public class KPICalculationService
{
    /// <summary>
    /// Calculates total patients metric
    /// </summary>
    public int CalculateTotalPatients(IEnumerable<AnalyticsMetric> metrics)
    {
        return metrics
            .Where(m => m.MetricName == "NewPatient")
            .Sum(m => (int)m.Value);
    }

    /// <summary>
    /// Calculates total appointments completed
    /// </summary>
    public int CalculateAppointmentsCompleted(IEnumerable<AnalyticsMetric> metrics)
    {
        return metrics
            .Where(m => m.MetricName == "AppointmentCompleted")
            .Sum(m => (int)m.Value);
    }

    /// <summary>
    /// Calculates total revenue
    /// </summary>
    public decimal CalculateRevenue(IEnumerable<AnalyticsMetric> metrics)
    {
        return metrics
            .Where(m => m.MetricName == "PaymentProcessed")
            .Sum(m => m.Value);
    }

    /// <summary>
    /// Calculates average appointment duration
    /// </summary>
    public double CalculateAverageAppointmentDuration(IEnumerable<AnalyticsMetric> metrics)
    {
        var durations = metrics
            .Where(m => m.MetricName == "AppointmentDuration")
            .Select(m => (double)m.Value)
            .ToList();

        if (!durations.Any())
            return 0;

        return durations.Average();
    }

    /// <summary>
    /// Calculates system uptime percentage
    /// </summary>
    public double CalculateSystemUptime(IEnumerable<AnalyticsMetric> metrics)
    {
        var uptimeMetrics = metrics
            .Where(m => m.MetricName == "SystemUptime")
            .Select(m => m.Value)
            .ToList();

        if (!uptimeMetrics.Any())
            return 100.0;

        return Math.Round(uptimeMetrics.Average() * 100, 2);
    }

    /// <summary>
    /// Calculates average API response time
    /// </summary>
    public double CalculateAverageResponseTime(IEnumerable<AnalyticsMetric> metrics)
    {
        var responseTimes = metrics
            .Where(m => m.MetricName == "ApiResponseTime")
            .Select(m => (double)m.Value)
            .ToList();

        if (!responseTimes.Any())
            return 0;

        return Math.Round(responseTimes.Average(), 2);
    }

    /// <summary>
    /// Aggregates metrics into complete KPI summary
    /// </summary>
    public KPISummary AggregateToKPISummary(
        DateTime summaryDate,
        IEnumerable<AnalyticsMetric> metrics,
        long tenantId)
    {
        var metricList = metrics.ToList();

        return new KPISummary
        {
            Id = Guid.NewGuid(),
            SummaryDate = summaryDate,
            TenantId = tenantId,
            TotalPatients = CalculateTotalPatients(metricList),
            AppointmentsCompleted = CalculateAppointmentsCompleted(metricList),
            AverageAppointmentDurationMinutes = CalculateAverageAppointmentDuration(metricList),
            RevenueInvoiced = CalculateRevenue(metricList),
            SystemUptime = CalculateSystemUptime(metricList),
            AverageResponseTimeMs = CalculateAverageResponseTime(metricList),
            CreatedAt = DateTime.UtcNow
        };
    }
}
