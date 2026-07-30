using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

/// <summary>
/// Add procedure command.
/// </summary>
public record AddProcedureCommand : ICommand
{
    public Guid ClinicalNoteId { get; init; }
    public string ProcedureName { get; init; } = string.Empty;
    public string ProcedureCode { get; init; } = string.Empty; // CPT or SNOMED
    public string Result { get; init; } = string.Empty;
}

