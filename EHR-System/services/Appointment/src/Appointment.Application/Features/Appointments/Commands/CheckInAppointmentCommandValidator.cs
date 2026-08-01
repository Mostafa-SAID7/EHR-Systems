using FluentValidation;
using EHRPlatform.Services.Appointment.Features.Appointments.Commands;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Validation;

/// <summary>
/// Validator for CheckInAppointmentCommand.
/// Ensures appointment ID is valid for check-in.
/// </summary>
public class CheckInAppointmentCommandValidator : AbstractValidator<CheckInAppointmentCommand>
{
    public CheckInAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required");
    }
}
