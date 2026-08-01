namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

using MediatR;

/// <summary>
/// Query to retrieve virus scan result for a document.
/// Returns latest scan status and threat details if any.
/// </summary>
public class GetVirusScanResultQuery : IRequest<VirusScanResultDto>
{
    public Guid DocumentId { get; set; }
}

public class VirusScanResultDto
{
    public Guid DocumentId { get; set; }
    public string Result { get; set; } = string.Empty; // CLEAN, INFECTED, ERROR
    public string? ThreatName { get; set; }
    public DateTime ScannedAt { get; set; }
    public string? ScanDetails { get; set; }
    public string ScannerName { get; set; } = string.Empty;
}
