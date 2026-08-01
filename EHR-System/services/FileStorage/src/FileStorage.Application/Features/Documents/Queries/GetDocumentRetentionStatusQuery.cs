namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

using MediatR;

/// <summary>
/// Query to check document retention status and deletion schedule.
/// Returns retention policy details and expiration date.
/// </summary>
public class GetDocumentRetentionStatusQuery : IRequest<DocumentRetentionStatusDto>
{
    public Guid DocumentId { get; set; }
}

public class DocumentRetentionStatusDto
{
    public Guid DocumentId { get; set; }
    public Guid? RetentionPolicyId { get; set; }
    public bool IsMarkedForDeletion { get; set; }
    public DateTime? ScheduledDeletionDate { get; set; }
    public int DaysUntilDeletion { get; set; }
    public string Status { get; set; } = string.Empty;
}
