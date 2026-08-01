using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

/// <summary>
/// Validator for GetPatientsQuery.
/// </summary>
public class GetPatientsValidator : AbstractValidator<GetPatientsQuery>
{
    public GetPatientsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("PageSize must not exceed 1000");

        RuleFor(x => x.Status)
            .Must(x => x == null || new[] { "Active", "Inactive", "Transferred" }.Contains(x))
            .WithMessage("Status must be Active, Inactive, or Transferred");
    }
}

