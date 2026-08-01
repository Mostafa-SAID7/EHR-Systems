using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

/// <summary>
/// Add condition command.
/// Adds a chronic condition or diagnosis to patient's medical history.
/// </summary>
public record AddConditionCommand : ICommand
{
    public Guid PatientId { get; init; }
    public string Condition { get; init; } = string.Empty;
    public string ICD10Code { get; init; } = string.Empty;
    public DateTime? OnsetDate { get; init; }
}



