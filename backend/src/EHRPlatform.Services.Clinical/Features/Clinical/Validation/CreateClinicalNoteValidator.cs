using FluentValidation;


namespace EHRPlatform.Services.Clinical.Features.Clinical.Validation;

public class CreateClinicalNoteValidator : AbstractValidator<CreateClinicalNoteCommand>
{
    public CreateClinicalNoteValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.EncounterType).NotEmpty();
    }
}

public class CreateClinicalNoteCommand
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string? EncounterType { get; set; }
}
