namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;

/// <summary>
/// Command to update or apply retention policy to a document.
/// Can extend retention period or schedule deletion.
/// </summary>
public class UpdateRetentionPolicyCommand : IRequest<UpdateRetentionPolicyResponse>
{
    public Guid DocumentId { get; set; }
    public Guid RetentionPolicyId { get; set; }
    public int RetentionDays { get; set; }
    public DateTime? CustomExpirationDate { get; set; }
}

public class UpdateRetentionPolicyResponse
{
    public Guid DocumentId { get; set; }
    public Guid RetentionPolicyId { get; set; }
    public int RetentionDays { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool Updated { get; set; }
}
