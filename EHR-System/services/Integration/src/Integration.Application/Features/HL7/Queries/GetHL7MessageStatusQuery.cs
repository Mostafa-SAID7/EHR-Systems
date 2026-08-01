namespace EHRPlatform.Services.Integration.Application.Features.HL7.Queries;

using MediatR;

/// <summary>
/// Query to get processing status of HL7 message.
/// Returns current status, parsed data, and any errors.
/// </summary>
public class GetHL7MessageStatusQuery : IRequest<HL7MessageStatusDto>
{
    public Guid MessageId { get; set; }
}

public class HL7MessageStatusDto
{
    public Guid MessageId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public bool IsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
