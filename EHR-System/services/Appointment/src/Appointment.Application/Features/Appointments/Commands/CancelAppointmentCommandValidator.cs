using FluentValidation;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Validation;

/// <summary>
/// Validator for CancelAppointmentCommand.
/// Ensures appointment ID is valid and reason meets minimum requirements.
/// </summary>
public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required")
            .MinimumLength(3)
            .WithMessage("Reason must be at least 3 characters")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}
