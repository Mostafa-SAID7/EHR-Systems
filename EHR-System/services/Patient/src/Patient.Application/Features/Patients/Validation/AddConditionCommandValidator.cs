using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

public class AddConditionCommandValidator : AbstractValidator<AddConditionCommand>
{
    public AddConditionCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Condition).NotEmpty();
        RuleFor(x => x.ICD10Code).Matches(@"^[A-Z][0-9]{2}(\.[0-9]{1,2})?$");
    }
}

