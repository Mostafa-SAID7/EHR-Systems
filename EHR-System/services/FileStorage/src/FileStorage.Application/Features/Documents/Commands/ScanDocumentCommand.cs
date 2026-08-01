namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;

/// <summary>
/// Command to initiate virus scanning on an uploaded document.
/// Triggers async scan via ClamAV or similar service.
/// </summary>
public class ScanDocumentCommand : IRequest<ScanDocumentResponse>
{
    public Guid DocumentId { get; set; }
    public string S3Key { get; set; } = string.Empty;
}

public class ScanDocumentResponse
{
    public Guid DocumentId { get; set; }
    public bool ScanInitiated { get; set; }
    public string ScanJobId { get; set; } = string.Empty;
    public DateTime ScanStartedAt { get; set; }
}
