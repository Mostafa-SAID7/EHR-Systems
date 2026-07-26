using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Billing.Domain.Enums;

namespace EHRPlatform.Services.Billing.Features.Claims.Commands;

/// <summary>
/// Command to request Prior Authorization for a clinical procedure or medication.
/// </summary>
public record RequestPriorAuthorizationCommand : ICommand
{
    public Guid ClinicalNoteId { get; init; }
    public Guid PatientId { get; init; }
    public string InsuranceProvider { get; init; } = string.Empty;
    public string MemberId { get; init; } = string.Empty;
    public string ProcedureCode { get; init; } = string.Empty;
    public string DiagnosisCode { get; init; } = string.Empty;
    public string ClinicalJustification { get; init; } = string.Empty;
}

public record PriorAuthorizationResponseDto
{
    public Guid Id { get; init; }
    public Guid ClinicalNoteId { get; init; }
    public Guid PatientId { get; init; }
    public string InsuranceProvider { get; init; } = string.Empty;
    public string ProcedureCode { get; init; } = string.Empty;
    public PriorAuthStatus Status { get; init; }
    public string? AuthorizationNumber { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? DecisionAt { get; init; }
    public string? DenialReason { get; init; }
}
