using FluentValidation;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Validation;

/// <summary>
/// Validator for GetReportsQuery.
/// </summary>
public class GetReportsValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("PageSize must not exceed 1000");
    }
}
