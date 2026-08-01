using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[0-9]{10,}$");
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Now);
        RuleFor(x => x.Gender).Must(g => new[] { "M", "F", "Other" }.Contains(g));
    }
}

