using Mapster;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;

/// <summary>
/// Mapster registration profile for Appointment entity mappings.
/// Single Responsibility: Configure all Appointment-related type mappings.
/// </summary>
public class AppointmentMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Appointment → AppointmentResponseDto
        config.NewConfig<ApptEntity, AppointmentResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PatientId, src => src.PatientId)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.ScheduledStart, src => src.ScheduledStart)
            .Map(dest => dest.ScheduledEnd, src => src.ScheduledEnd)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.AppointmentType, src => src.AppointmentType)
            .Map(dest => dest.ReasonForVisit, src => src.ReasonForVisit)
            .Map(dest => dest.Notes, src => src.Notes);
    }
}
