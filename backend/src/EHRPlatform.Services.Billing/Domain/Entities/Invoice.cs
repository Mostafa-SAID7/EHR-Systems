using EHRPlatform.Common.Entities;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Billing.Domain.Events;
using EHRPlatform.Services.Billing.Domain.Enums;

namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Invoice aggregate root.
/// Single Responsibility: Manage invoice lifecycle - creation, totals, payments, insurance submission.
/// </summary>
public class Invoice : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceResponsibility { get; set; }
    public decimal PatientResponsibility { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue => TotalAmount - AmountPaid;
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? Notes { get; set; }

    public ICollection<LineItem> LineItems { get; } = new List<LineItem>();
    public ICollection<Payment> Payments { get; } = new List<Payment>();
    public ICollection<InsuranceClaim> InsuranceClaims { get; } = new List<InsuranceClaim>();

    private readonly List<IntegrationEvent> _domainEvents = new();

    public void AddLineItem(string description, string cptCode, decimal quantity, decimal unitPrice)
    {
        var lineItem = new LineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = Id,
            Description = description,
            CPTCode = cptCode,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = quantity * unitPrice
        };
        LineItems.Add(lineItem);
    }

    public void CalculateTotals()
    {
        SubTotal = LineItems.Sum(l => l.Amount);
        TaxAmount = SubTotal * 0.08m;
        TotalAmount = SubTotal + TaxAmount;
    }

    public void RecordPayment(decimal amount, string method, string reference = "")
    {
        if (amount <= 0)
            throw new InvalidOperationException("Payment amount must be positive");

        if (AmountPaid + amount > TotalAmount)
            throw new InvalidOperationException("Payment exceeds invoice total");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = Id,
            Amount = amount,
            Method = method,
            Reference = reference,
            ReceivedAt = DateTime.UtcNow
        };
        Payments.Add(payment);

        AmountPaid += amount;

        var newStatus = AmountPaid >= TotalAmount ? "Paid" : "PartiallyPaid";
        RaiseEvent(new PaymentReceivedEvent(Id, PatientId, amount, newStatus));
    }

    public void SubmitToInsurance(string provider, string policyNumber)
    {
        if (Status != "Draft")
            throw new InvalidOperationException("Only draft invoices can be submitted");

        InsuranceProvider = provider;
        InsurancePolicyNumber = policyNumber;
        Status = "Submitted";

        var claim = new InsuranceClaim
        {
            Id = Guid.NewGuid(),
            InvoiceId = Id,
            InsuranceProvider = provider,
            ClaimNumber = GenerateClaimNumber(),
            SubmittedAt = DateTime.UtcNow,
            Status = ClaimStatus.Submitted,
            Amount = InsuranceResponsibility
        };
        InsuranceClaims.Add(claim);

        RaiseEvent(new InvoiceSubmittedEvent(Id, PatientId, InsuranceResponsibility, provider));
    }

    public void MarkPaid()
    {
        if (Status == "Paid")
            return;

        Status = "Paid";
        RaiseEvent(new InvoicePaidEvent(Id, PatientId, TotalAmount));
    }

    public void Cancel(string reason = "")
    {
        if (Status == "Paid")
            throw new InvalidOperationException("Cannot cancel paid invoice");

        Status = "Cancelled";
        RaiseEvent(new InvoiceCancelledEvent(Id, PatientId, reason));
    }

    private string GenerateClaimNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"CLM-{timestamp}-{random}";
    }

    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
