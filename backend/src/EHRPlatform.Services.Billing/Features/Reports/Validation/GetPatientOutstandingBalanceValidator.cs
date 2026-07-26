using FluentValidation;
using EHRPlatform.Services.Billing.Features.Reports.Queries;

namespace EHRPlatform.Services.Billing.Features.Reports.Validation;

/// <summary>
/// Validator for GetPatientOutstandingBalanceQuery.
/// Single Responsibility: Enforce patient ID requirement for outstanding balance queries.
/// </summary>
public class GetPatientOutstandingBalanceValidator : AbstractValidator<GetPatientOutstandingBalanceQuery>
{
    public GetPatientOutstandingBalanceValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
