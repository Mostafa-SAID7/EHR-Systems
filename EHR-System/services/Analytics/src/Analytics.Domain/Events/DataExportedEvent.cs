namespace EHRPlatform.Services.Analytics.Domain.Events;

/// <summary>
/// Domain event raised when data is exported
/// </summary>
public record DataExportedEvent(
    Guid ExportId,
    string FileName,
    string Format,
    DateTime FromDate,
    DateTime ToDate,
    long FileSize,
    Guid ExportedBy,
    long TenantId,
    DateTime ExportedAt);

