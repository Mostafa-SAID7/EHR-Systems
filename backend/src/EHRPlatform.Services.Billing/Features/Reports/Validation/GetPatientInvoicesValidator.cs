using FluentValidation;
using EHRPlatform.Services.Billing.Features.Reports.Queries;

namespace EHRPlatform.Services.Billing.Features.Reports.Validation;

/// <summary>
/// Validator for GetPatientInvoicesQuery.
/// Single Responsibility: Enforce pagination and patient ID constraints for invoice queries.
/// </summary>
public class GetPatientInvoicesValidator : AbstractValidator<GetPatientInvoicesQuery>
{
    public GetPatientInvoicesValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}
