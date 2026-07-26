using FluentValidation;
using EHRPlatform.Services.Billing.Features.Reports.Queries;

namespace EHRPlatform.Services.Billing.Features.Reports.Validation;

/// <summary>
/// Validator for GetBillingReportQuery.
/// Single Responsibility: Enforce date range consistency for billing report queries.
/// </summary>
public class GetBillingReportValidator : AbstractValidator<GetBillingReportQuery>
{
    public GetBillingReportValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("StartDate must be before EndDate");
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate");
    }
}
