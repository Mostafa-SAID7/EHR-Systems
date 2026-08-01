using EHRPlatform.BuildingBlocks.Contracts.DTOs;

namespace EHRPlatform.Services.Billing.Application.Invoicing.Responses;

/// <summary>
/// Invoice response DTO with slug support for InvoiceNumber.
/// Contains complete invoice information for API responses.
/// Enables URL-friendly invoice lookup via InvoiceNumber slug.
/// </summary>
public class InvoiceResponseDto : StatusDto
{
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Invoice number (unique identifier, human-readable).
    /// Used as basis for invoice slug.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-friendly slug for InvoiceNumber (e.g., "inv-20250115-001234").
    /// Enables slug-based lookup: GET /api/v1/billing/invoices/invoice-number/{invoiceNumberSlug}
    /// </summary>
    public string? InvoiceNumberSlug { get; set; }
    
    public Guid? AppointmentId { get; set; }
    public DateTime ServiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceResponsibility { get; set; }
    public decimal PatientResponsibility { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<LineItemDto> LineItems { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<InsuranceClaimDto> Claims { get; set; } = new();
}


