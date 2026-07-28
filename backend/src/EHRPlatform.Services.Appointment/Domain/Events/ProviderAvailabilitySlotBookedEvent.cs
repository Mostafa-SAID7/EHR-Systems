using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when a provider availability slot is booked.
/// </summary>
public class ProviderAvailabilitySlotBookedEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the availability identifier.
    /// </summary>
    public Guid AvailabilityId { get; set; }

    /// <summary>
    /// Gets the appointment identifier that booked this slot.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets the patient identifier.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Gets the current bookings count.
    /// </summary>
    public int CurrentBookings { get; set; }

    /// <summary>
    /// Gets the maximum appointments per slot.
    /// </summary>
    public int? MaxAppointmentsPerSlot { get; set; }

    /// <summary>
    /// Gets the booking time.
    /// </summary>
    public DateTime BookedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAvailabilitySlotBookedEvent"/> class.
    /// </summary>
    public ProviderAvailabilitySlotBookedEvent(
        Guid availabilityId,
        Guid appointmentId,
        Guid providerId,
        Guid patientId,
        int currentBookings,
        int? maxAppointmentsPerSlot,
        DateTime bookedAt)
    {
        AvailabilityId = availabilityId;
        AppointmentId = appointmentId;
        ProviderId = providerId;
        PatientId = patientId;
        CurrentBookings = currentBookings;
        MaxAppointmentsPerSlot = maxAppointmentsPerSlot;
        BookedAt = bookedAt;
    }
}
