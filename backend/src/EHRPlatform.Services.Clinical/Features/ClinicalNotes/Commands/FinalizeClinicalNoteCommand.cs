using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

/// <summary>
/// Finalize clinical note command.
/// Locks note for editing, publishes event.
/// </summary>
public record FinalizeClinicalNoteCommand : ICommand<ClinicalNoteResponse>
{
    public Guid ClinicalNoteId { get; init; }
}

