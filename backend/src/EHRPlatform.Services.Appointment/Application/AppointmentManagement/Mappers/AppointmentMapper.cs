using Mapster;
using EHRPlatform.Common.Mapping;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using Microsoft.Extensions.Logging;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;
using ProvAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;

/// <summary>
/// Appointment mapper. Single Responsibility: convert Appointment domain model to DTOs.
/// </summary>
public class AppointmentMapper : MappingServiceBase<ApptEntity, AppointmentResponseDto>
{
    public AppointmentMapper(ILogger<AppointmentMapper> logger) : base(logger) { }

    public AppointmentResponseDto MapToResponseDto(ApptEntity appointment) => MapSingleToDto(appointment);

    public List<AppointmentResponseDto> MapToResponseDtoList(ICollection<ApptEntity> appointments)
    {
        Logger.LogDebug("Mapping {Count} appointments to response DTO list", appointments.Count);
        return appointments.Adapt<List<AppointmentResponseDto>>();
    }

    public PagedResult<AppointmentResponseDto> MapToPagedResult(IList<ApptEntity> appointments, int total, int pageNumber, int pageSize)
    {
        Logger.LogDebug("Mapping {Count} appointments to paged result", appointments.Count);
        return PagedResult<AppointmentResponseDto>.Create(
            appointments.Adapt<List<AppointmentResponseDto>>(),
            total,
            pageNumber,
            pageSize);
    }

    public ProviderAppointmentCalendarDto MapToProviderCalendarDto(Guid providerId, DateTime date, IList<ApptEntity> appointments)
        => new()
        {
            ProviderId = providerId, Date = date,
            Slots = appointments.Select(a => new AppointmentSlotDto
            {
                AppointmentId = a.Id, PatientId = a.PatientId,
                Start = a.ScheduledStart, End = a.ScheduledEnd,
                AppointmentType = a.AppointmentType, Status = a.Status
            }).ToList()
        };

    public ProviderAvailabilityListDto MapToAvailabilityListDto(Guid providerId, IList<ProvAvailEntity> slots)
        => new()
        {
            ProviderId = providerId,
            Slots = slots.Select(s => new ProviderAvailabilitySlotDto
            {
                Id = s.Id, SlotStart = s.SlotStart, SlotEnd = s.SlotEnd,
                IsRecurring = s.IsRecurring, RecurrencePattern = s.RecurrencePattern,
                MaxAppointmentsPerSlot = s.MaxAppointmentsPerSlot, CurrentBookings = s.CurrentBookings,
                HasAvailability = s.HasAvailability()
            }).ToList()
        };
}
