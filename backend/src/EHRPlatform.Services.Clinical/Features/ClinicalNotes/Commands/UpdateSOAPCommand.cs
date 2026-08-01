using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

/// <summary>
/// Update SOAP note command.
/// </summary>
public record UpdateSOAPCommand : ICommand
{
    public Guid ClinicalNoteId { get; init; }
    public string? Subjective { get; init; }
    public string? Objective { get; init; }
    public string? Assessment { get; init; }
    public string? Plan { get; init; }
}


