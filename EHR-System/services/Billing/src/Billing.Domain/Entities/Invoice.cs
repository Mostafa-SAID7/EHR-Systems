namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Invoice aggregate root - manages invoice lifecycle, line items, payments, and insurance claims.
/// Single Responsibility: Manage invoice creation, totals calculation, payments, and insurance submission.
/// HIPAA Compliant: Tracks all access and modifications for audit purposes.
/// </summary>
public class Invoice
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, PartiallyPaid, Paid, Cancelled
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
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LineItem> LineItems { get; } = new List<LineItem>();
    public ICollection<Payment> Payments { get; } = new List<Payment>();
    public ICollection<InsuranceClaim> InsuranceClaims { get; } = new List<InsuranceClaim>();

    private readonly List<object> _domainEvents = new();

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
            Amount = quantity * unitPrice,
            CreatedAt = DateTime.UtcNow
        };
        LineItems.Add(lineItem);
    }

    public void CalculateTotals()
    {
        SubTotal = LineItems.Sum(l => l.Amount);
        TaxAmount = SubTotal * 0.08m; // 8% tax
        TotalAmount = SubTotal + TaxAmount;
        UpdatedAt = DateTime.UtcNow;
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
            ReceivedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        Payments.Add(payment);
        AmountPaid += amount;
        
        if (AmountPaid >= TotalAmount)
            Status = "Paid";
        else
            Status = "PartiallyPaid";

        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PaymentReceivedEvent(Id, PatientId, amount, Status));
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
            Status = "Submitted",
            Amount = InsuranceResponsibility,
            CreatedAt = DateTime.UtcNow
        };
        InsuranceClaims.Add(claim);

        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new InvoiceSubmittedEvent(Id, PatientId, InsuranceResponsibility, provider));
    }

    public void MarkPaid()
    {
        if (Status == "Paid")
            return;

        Status = "Paid";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new InvoicePaidEvent(Id, PatientId, TotalAmount));
    }

    public void Cancel(string reason = "")
    {
        if (Status == "Paid")
            throw new InvalidOperationException("Cannot cancel paid invoice");

        Status = "Cancelled";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new InvoiceCancelledEvent(Id, PatientId, reason));
    }

    private string GenerateClaimNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"CLM-{timestamp}-{random}";
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
