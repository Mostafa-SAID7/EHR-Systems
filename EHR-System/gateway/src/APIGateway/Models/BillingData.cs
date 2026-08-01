namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Billing data from Billing Service.
/// </summary>
public class BillingData
{
    public string PatientId { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal OutstandingBalance { get; set; }
    public List<InvoiceData> RecentInvoices { get; set; } = new();
}
