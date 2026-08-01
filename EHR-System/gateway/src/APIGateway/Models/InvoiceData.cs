namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Invoice data from Billing Service.
/// </summary>
public class InvoiceData
{
    public string Id { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Paid { get; set; }
    public string Status { get; set; } = string.Empty;
}
