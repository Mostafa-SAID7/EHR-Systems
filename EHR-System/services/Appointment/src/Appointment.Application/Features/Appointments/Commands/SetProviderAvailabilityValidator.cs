using FluentValidation;
using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Commands;

namespace EHRPlatform.Services.Appointment.Features.ProviderAvailability.Validation;

/// <summary>
/// Validator for SetProviderAvailabilityCommand.
/// Ensures availability slots have valid times and constraints.
/// </summary>
public class SetProviderAvailabilityValidator : AbstractValidator<SetProviderAvailabilityCommand>
{
    public SetProviderAvailabilityValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("Provider ID is required");

        RuleFor(x => x.SlotStart)
            .NotEmpty()
            .WithMessage("Slot start time is required")
            .GreaterThan(x => DateTime.UtcNow)
            .WithMessage("Slot start time cannot be in the past");

        RuleFor(x => x.SlotEnd)
            .NotEmpty()
            .WithMessage("Slot end time is required")
            .GreaterThan(x => x.SlotStart)
            .WithMessage("Slot end time must be after slot start time");

        RuleFor(x => x.RecurrencePattern)
            .Must(BeValidRecurrencePattern)
            .WithMessage("Invalid recurrence pattern (Daily, Weekly, Monthly)")
            .When(x => x.IsRecurring);

        RuleFor(x => x.MaxAppointmentsPerSlot)
            .GreaterThan(0)
            .WithMessage("Max appointments per slot must be greater than 0")
            .When(x => x.MaxAppointmentsPerSlot.HasValue);
    }

    private bool BeValidRecurrencePattern(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;

        var validPatterns = new[] { "Daily", "Weekly", "BiWeekly", "Monthly", "Yearly" };
        return validPatterns.Contains(pattern, StringComparer.OrdinalIgnoreCase);
    }
}
