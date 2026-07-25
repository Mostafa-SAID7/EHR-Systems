using FluentValidation;
using EHRPlatform.Services.Patient.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Features.Patients.Validation;

public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}
