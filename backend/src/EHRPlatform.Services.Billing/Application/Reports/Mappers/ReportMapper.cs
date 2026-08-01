using Mapster;
using EHRPlatform.BuildingBlocks.Common.Application.Mapping;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Application.Reports.Mappers;

/// <summary>
/// Report Mapper
/// Single Responsibility: Convert domain models to report aggregate DTOs.
/// Handles only Reports feature mappings - balance, metrics, aggregations.
/// </summary>
public class ReportMapper : MappingServiceBase<Invoice, OutstandingBalanceDto>
{
    public ReportMapper(ILogger<ReportMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map invoices with enriched balance and aging information.
    /// </summary>
    public OutstandingBalanceDto MapToOutstandingBalanceDto(
        Guid patientId,
        ICollection<Invoice> invoices)
    {
        Logger.LogDebug("Mapping outstanding balance for patient {PatientId}", patientId);

        var invoiceDtos = invoices.Adapt<List<InvoiceResponseDto>>();
        var totalBalance = invoices.Sum(i => i.BalanceDue);
        var overdueInvoices = invoices.Count(i => i.DueDate < DateTime.UtcNow && i.Status != "Paid");
        var overdueAmount = invoices
            .Where(i => i.DueDate < DateTime.UtcNow && i.Status != "Paid")
            .Sum(i => i.BalanceDue);

        return new OutstandingBalanceDto
        {
            PatientId = patientId,
            TotalBalance = totalBalance,
            OverdueInvoices = overdueInvoices,
            OverdueAmount = overdueAmount,
            Invoices = invoiceDtos
        };
    }

    /// <summary>
    /// Map invoices to billing report DTO with metrics and aggregations.
    /// </summary>
    public BillingReportDto MapToBillingReportDto(
        DateTime startDate,
        DateTime endDate,
        ICollection<Invoice> invoices)
    {
        Logger.LogDebug("Mapping billing report for {StartDate} to {EndDate}", startDate, endDate);

        var totalInvoiced = invoices.Sum(i => i.TotalAmount);
        var totalPaid = invoices.Sum(i => i.AmountPaid);
        var totalOutstanding = invoices.Sum(i => i.BalanceDue);
        var collectionRate = totalInvoiced > 0 ? (double)(totalPaid / totalInvoiced) : 0;

        var dailyMetrics = invoices
            .GroupBy(i => i.ServiceDate.Date)
            .Select(g => new BillingMetricDto
            {
                Date = g.Key,
                Invoiced = g.Sum(i => i.TotalAmount),
                Paid = g.Sum(i => i.AmountPaid),
                InsuranceClaims = g.Sum(i => i.TotalAmount) // placeholder
            })
            .OrderBy(m => m.Date)
            .ToList();

        return new BillingReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            TotalOutstanding = totalOutstanding,
            TotalInsuranceClaims = 0, // calculate from claims data
            InvoiceCount = invoices.Count,
            PatientCount = invoices.Select(i => i.PatientId).Distinct().Count(),
            CollectionRate = collectionRate,
            DailyMetrics = dailyMetrics
        };
    }
}


