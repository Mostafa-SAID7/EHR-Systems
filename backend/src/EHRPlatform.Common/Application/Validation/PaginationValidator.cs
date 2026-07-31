#nullable enable

using FluentValidation;
using EHRPlatform.Common.Shared.DTOs;

namespace EHRPlatform.Common.Application.Common.Validators;

/// <summary>
/// Validator for PaginationRequest to ensure valid page number and size.
/// Use across all services for consistent pagination validation.
/// </summary>
public class PaginationValidator : AbstractValidator<PaginationRequest>
{
    public PaginationValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100 items");

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrEmpty(x) || x == "asc" || x == "desc")
            .WithMessage("Sort order must be 'asc' or 'desc'");
    }
}

