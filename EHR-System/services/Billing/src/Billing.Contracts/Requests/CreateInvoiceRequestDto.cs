namespace EHRPlatform.Services.Billing.Contracts.Requests;

/// <summary>
/// Request DTO for creating a new invoice.
/// </summary>
public class CreateInvoiceRequestDto
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? Notes { get; set; }
    public List<LineItemRequestDto> LineItems { get; set; } = new();
}

public class LineItemRequestDto
{
    public string Description { get; set; } = string.Empty;
    public string CPTCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
