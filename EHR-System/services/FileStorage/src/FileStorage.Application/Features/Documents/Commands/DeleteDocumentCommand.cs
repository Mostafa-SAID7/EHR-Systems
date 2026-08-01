namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;

/// <summary>
/// Command to delete a document based on retention policy.
/// Can be immediate or scheduled for future deletion.
/// </summary>
public class DeleteDocumentCommand : IRequest<DeleteDocumentResponse>
{
    public Guid DocumentId { get; set; }
    public string Reason { get; set; } = string.Empty; // Retention expired, Patient request, etc.
    public bool IsImmediate { get; set; } = false;
}

public class DeleteDocumentResponse
{
    public Guid DocumentId { get; set; }
    public bool DeleteScheduled { get; set; }
    public DateTime? ScheduledDeletionDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
