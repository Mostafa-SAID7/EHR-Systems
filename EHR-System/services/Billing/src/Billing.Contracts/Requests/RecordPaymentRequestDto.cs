namespace EHRPlatform.Services.Billing.Contracts.Requests;

/// <summary>
/// Request DTO for recording a payment on an invoice.
/// </summary>
public class RecordPaymentRequestDto
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // CreditCard, Check, ACH, Wire, etc.
    public string? Reference { get; set; } // Transaction ID, Check #, Wire confirmation, etc.
}
