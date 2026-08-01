namespace EHRPlatform.Services.Analytics.Contracts.Requests;

/// <summary>
/// Request DTO for exporting analytics data
/// </summary>
public class ExportDataRequestDto
{
    /// <summary>Gets or sets start date for export.</summary>
    public DateTime FromDate { get; set; }

    /// <summary>Gets or sets end date for export.</summary>
    public DateTime ToDate { get; set; }

    /// <summary>Gets or sets export format (CSV, Excel, JSON, PDF).</summary>
    public string Format { get; set; } = "CSV";

    /// <summary>Gets or sets optional filters for export.</summary>
    public string? Filters { get; set; }
}
