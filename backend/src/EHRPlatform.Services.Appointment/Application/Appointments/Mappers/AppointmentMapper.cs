using Mapster;
using EHRPlatform.Common.Application.Mapping;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using Microsoft.Extensions.Logging;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;
using ProvAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Application.Appointments.Mappers;

/// <summary>
/// Appointment mapper.
/// Converts Appointment domain entities to DTOs.
/// Single Responsibility: Mapping Appointment aggregate to application layer DTOs.
/// </summary>
public class AppointmentMapper : MappingServiceBase<ApptEntity, AppointmentResponseDto>
{
    public AppointmentMapper(ILogger<AppointmentMapper> logger) : base(logger) { }

    /// <summary>
    /// Maps appointment entity to response DTO.
    /// </summary>
    public AppointmentResponseDto MapToResponseDto(ApptEntity appointment) => MapSingleToDto(appointment);

    /// <summary>
    /// Maps collection of appointments to response DTOs.
    /// </summary>
    public List<AppointmentResponseDto> MapToResponseDtoList(ICollection<ApptEntity> appointments)
    {
        Logger.LogDebug("Mapping {Count} appointments to response DTO list", appointments.Count);
        return appointments.Adapt<List<AppointmentResponseDto>>();
    }

    /// <summary>
    /// Maps appointments to paged result DTO.
    /// </summary>
    public PagedResult<AppointmentResponseDto> MapToPagedResult(
        IList<ApptEntity> appointments,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} appointments to paged result", appointments.Count);
        return PagedResult<AppointmentResponseDto>.Create(
            appointments.Adapt<List<AppointmentResponseDto>>(),
            total,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Maps appointment to detailed response DTO with reminders and computed fields.
    /// </summary>
    public AppointmentDetailedResponseDto MapToDetailedResponseDto(ApptEntity appointment)
    {
        Logger.LogDebug("Mapping appointment {Id} to detailed response DTO", appointment.Id);
        var dto = appointment.Adapt<AppointmentDetailedResponseDto>();
        dto.Reminders = appointment.Reminders
            .Select(r => new AppointmentReminderDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                ReminderDateTime = r.ReminderTime,
                Channel = r.Method.ToString(),
                Status = r.Status.ToString(),
                SentAt = r.SentAt
            })
            .ToList();
        return dto;
    }

    /// <summary>
    /// Maps appointments to provider calendar DTO.
    /// </summary>
    public ProviderAppointmentCalendarDto MapToProviderCalendarDto(
        Guid providerId,
        DateTime date,
        IList<ApptEntity> appointments)
    {
        Logger.LogDebug("Mapping {Count} appointments to provider calendar DTO for date {Date}", 
            appointments.Count, date);
        
        return new()
        {
            ProviderId = providerId,
            Date = date,
            Slots = appointments.Select(a => new AppointmentSlotDto
            {
                AppointmentId = a.Id,
                PatientId = a.PatientId,
                Start = a.ScheduledStart,
                End = a.ScheduledEnd,
                AppointmentType = a.AppointmentType.ToString(),
                Status = a.Status.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Maps appointment to command response DTO.
    /// </summary>
    public AppointmentCommandDto MapToCommandDto(ApptEntity appointment)
    {
        Logger.LogDebug("Mapping appointment {Id} to command response DTO", appointment.Id);
        return new()
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            AppointmentType = appointment.AppointmentType.ToString(),
            ReasonForVisit = appointment.ReasonForVisit,
            Notes = appointment.Notes,
            DurationMinutes = appointment.DurationMinutes
        };
    }
}

